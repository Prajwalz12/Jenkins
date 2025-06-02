using Extensions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Serilog.Sinks.AzureDataExplorer;
using Serilog.Sinks.AzureDataExplorer.Extensions;
using System.Threading.Tasks;

namespace EventManagerWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .UseSerilog(configureLogger: (context, configuration) =>
        {
            var userName = context.Configuration["ElasticConfiguration:User"];
            var password = context.Configuration["ElasticConfiguration:Password"];
            if (!String.IsNullOrEmpty(userName) && !String.IsNullOrEmpty(password))
            {
                ElasticsearchConfigurationWithConnectionSetting(context, configuration);
            }
            else
            {
                ElasticsearchConfigurationWithoutConnectionSetting(context, configuration);
            }
        })
        .ConfigureServices((hostContext, services) =>
        {
            services.AddDomainServices(hostContext.Configuration);
            services.AddHostedService<ManageEventWorker>();
        });
        private static void ElasticsearchConfigurationWithConnectionSetting(HostBuilderContext context, LoggerConfiguration configuration)
        {
            var uris = context.Configuration
            .GetSection("ElasticConfiguration:Uri")
            .GetChildren()
            .Select(x => new Uri(x.Value))
            .ToArray();
            configuration.Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .WriteTo.Console()
                        //.WriteTo.AzureDataExplorerSink(new AzureDataExplorerSinkOptions
                        //{
                        //    IngestionEndpointUri = context.Configuration["ADXKeys:IngestionEndpointUri"],
                        //    DatabaseName = context.Configuration["ADXKeys:DatabaseName"],
                        //    TableName = context.Configuration["ADXKeys:TableName"],
                        //    //FlushImmediately = Convert.ToBoolean(context.Configuration["ADXKeys:FlushImmediately"]),
                        //    //BufferBaseFileName = context.Configuration["ADXKeys:BufferBaseFileName"],
                        //    // BatchPostingLimit = 5,
                        //    // Period = TimeSpan.FromSeconds(2),
                        //    //QueueSizeLimit = 100000,

                        //    // BufferFileRollingInterval = RollingInterval.Minute,
                        //    ColumnsMapping = new[]
                                            //             {
                        // new SinkColumnMapping { ColumnName ="Timestamp", ColumnType ="datetime", ValuePath = "$.Timestamp" } ,
                        // new SinkColumnMapping { ColumnName ="Level", ColumnType ="string", ValuePath = "$.Level" } ,
                        // new SinkColumnMapping { ColumnName ="Message", ColumnType ="string", ValuePath = "$.Message" } ,
                        // new SinkColumnMapping { ColumnName ="Exception", ColumnType ="string", ValuePath = "$.ExceptionEx" } ,
                        // new SinkColumnMapping { ColumnName ="Properties", ColumnType ="dynamic", ValuePath = "$.Properties" } ,
                        // new SinkColumnMapping { ColumnName ="Position", ColumnType ="dynamic", ValuePath = "$.Properties.Position" } ,
                        // new SinkColumnMapping { ColumnName ="Elapsed", ColumnType = "int", ValuePath = "$.Properties.Elapsed" },
                                         //                }
                        //}.WithAadApplicationKey(context.Configuration["ADXKeys:ApplicationClientId"], context.Configuration["ADXKeys:ApplicationKey"], context.Configuration["ADXKeys:Authority"]))



            .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(nodes: uris)
            {
                AutoRegisterTemplate = true,
                ModifyConnectionSettings = x => x.BasicAuthentication(context.Configuration["ElasticConfiguration:User"], context.Configuration["ElasticConfiguration:Password"]),
                //IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}-logs-{context.HostingEnvironment.EnvironmentName?.ToLower().Replace(oldValue: ".", newValue: "-")}-{DateTime.UtcNow:yyyy-MM}"
                IndexFormat = Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-") + "-logs-" + context.HostingEnvironment.EnvironmentName?.ToLower().Replace(oldValue: ".", newValue: "-") + "-{0:yyyy-MM-dd}"

            })

            .Enrich.WithProperty(name: "Environment", context.HostingEnvironment.EnvironmentName)
            .ReadFrom.Configuration(context.Configuration);
        }
        private static void ElasticsearchConfigurationWithoutConnectionSetting(HostBuilderContext context, LoggerConfiguration configuration)
        {
            var uris = context.Configuration
            .GetSection("ElasticConfiguration:Uri")
            .GetChildren()
            .Select(x => new Uri(x.Value))
            .ToArray();
            configuration.Enrich.FromLogContext()
            .Enrich.WithMachineName()
                        //.WriteTo.Console().WriteTo.AzureDataExplorerSink(new AzureDataExplorerSinkOptions
                        //{
                        //    IngestionEndpointUri = context.Configuration["ADXKeys:IngestionEndpointUri"],
                        //    DatabaseName = context.Configuration["ADXKeys:DatabaseName"],
                        //    TableName = context.Configuration["ADXKeys:TableName"],
                        //    FlushImmediately = Convert.ToBoolean(context.Configuration["ADXKeys:FlushImmediately"]),
                        //    BufferBaseFileName = context.Configuration["ADXKeys:BufferBaseFileName"],
                        //   // BatchPostingLimit = 5,
                        //    // Period = TimeSpan.FromSeconds(2),
                        //    //QueueSizeLimit = 100000,

                        //    BufferFileRollingInterval = RollingInterval.Minute,
                        //    ColumnsMapping = new[]
                                                    //     {
                        // new SinkColumnMapping { ColumnName ="Timestamp", ColumnType ="datetime", ValuePath = "$.Timestamp" } ,
                        // new SinkColumnMapping { ColumnName ="Level", ColumnType ="string", ValuePath = "$.Level" } ,
                        // new SinkColumnMapping { ColumnName ="Message", ColumnType ="string", ValuePath = "$.Message" } ,
                        // new SinkColumnMapping { ColumnName ="Exception", ColumnType ="string", ValuePath = "$.ExceptionEx" } ,
                        // new SinkColumnMapping { ColumnName ="Properties", ColumnType ="dynamic", ValuePath = "$.Properties" } ,
                        // new SinkColumnMapping { ColumnName ="Position", ColumnType ="dynamic", ValuePath = "$.Properties.Position" } ,
                        // new SinkColumnMapping { ColumnName ="Elapsed", ColumnType = "int", ValuePath = "$.Properties.Elapsed" },
                                                     //    }
                        //}.WithAadApplicationKey(context.Configuration["ADXKeys:ApplicationClientId"], context.Configuration["ADXKeys:ApplicationKey"], context.Configuration["ADXKeys:Authority"]))

            .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(nodes: uris)
            {
                AutoRegisterTemplate = true,
                //IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}-logs-{context.HostingEnvironment.EnvironmentName?.ToLower().Replace(oldValue: ".", newValue: "-")}-{DateTime.UtcNow:yyyy-MM}"
                IndexFormat = Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-") + "-logs-" + context.HostingEnvironment.EnvironmentName?.ToLower().Replace(oldValue: ".", newValue: "-") + "-{0:yyyy-MM-dd}"
            }
            )
            .Enrich.WithProperty(name: "Environment", context.HostingEnvironment.EnvironmentName)
            .ReadFrom.Configuration(context.Configuration);
        }
    }
}
