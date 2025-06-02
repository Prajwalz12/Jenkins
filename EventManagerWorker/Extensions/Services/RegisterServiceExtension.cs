using Confluent.Kafka;
using Domain.Processors;
using Domain.Services;
using Domain.Services.Kafka;
using Domain.Services.ProxyClientServices;
using Domain.Settings;
using EventManagerWorker.Processors;
using EventManagerWorker.Services.MongoServices;
using EventManagerWorker.Services.MongoServices.OncePerDestination;
using EventManagerWorker.Services.MongoServices.ReferralTable;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Extensions.Services
{
    public static class RegisterServiceExtension
    {
        private const string DBServiceBaseAddress = @"Services:DBService:BaseAddress";
        private const string MongoServiceBaseAddress = @"Services:MongoService:BaseAddress";
        private const string EventManagerApiAddress = @"Services:EvenetManagerService:BaseAddress";
        private const string UtilServiceApiAddress = @"Services:UtilService:BaseAddress";
        private const string LobMonthlyBudgetCappingApiAddress = @"Services:LobMonthlyBudgetCappingApi:BaseAddress";
        private const string ApplicationJson = @"application/json";

        public static IServiceCollection AddDomainServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ISubscriptionService, SubscriptionService>();
            services.AddSingleton<ICustomerService, CustomerService>();
            services.AddSingleton<ICustomerEventService, CustomerEventService>();
            services.AddSingleton<ICustomerVersionService, CustomerVersionService>();
            services.AddSingleton<ICustomerSummaryService, CustomerSummaryService>();
            services.AddSingleton<ITransactionService, TransactionService>();
            services.AddSingleton<IGroupCampaignTransactionService, GroupCampaignTransactionService>();
            services.AddSingleton<ITransactionRewardService, TransactionRewardService>();
            services.AddSingleton<ITempRewardService, TempRewardService>();
            services.AddSingleton<IMissedTransactionService, MissedTransactionService>();
            services.AddSingleton<ICampaignService, CampaignService>();
            services.AddSingleton<ILoyaltyFraudManagementService, LoyaltyFraudManagementService>();
            services.AddSingleton<IOfferMapService, OfferMapService>();
            services.AddSingleton<ICumulativeTransactionService, CumulativeTransactionService>();
            services.AddSingleton<WebUIDatabaseService>();
            //services.AddTransient<WebUIDBContext>(_ => new WebUIDBContext(configuration["DatabaseSettings:MySql:WebUIDBConnectionString"]));
            services.AddSingleton<Processor>();
            services.AddSingleton<ProcessorService>();
            services.AddSingleton<IReferralService, ReferralService>();
            services.AddSingleton<DirectCampaignService>();
            services.AddSingleton<LockCampaignService>();
            services.AddSingleton<ReferralUnlockCampaignService>();
            services.AddSingleton<UnlockCampaignService>();
            //services.AddSingleton<QueueServiceHelper>();
            services.AddSingleton<CustomerService>();
            services.AddSingleton<CustomerVersionService>();
            services.Configure<DatabaseSettings>(configuration.GetSection(nameof(DatabaseSettings)));
            services.AddSingleton<IDatabaseSettings>(sp => sp.GetRequiredService<IOptions<DatabaseSettings>>().Value);
            services.AddSingleton<IMerchantMaster, MerchantMasterService>();
            services.AddSingleton<IOncePerDestionVpaId, OncePerDestinationVpaIdService>();
            services.AddSingleton<IFraudRewardService, FraudRewardService>();

            var producerConfig = new ProducerConfig();
            var consumerConfig = new ConsumerConfig();

            configuration.Bind("Producer", producerConfig);
            configuration.Bind("Consumer", consumerConfig);

            consumerConfig.AutoOffsetReset = AutoOffsetReset.Earliest;

            services.AddSingleton<ProducerConfig>(producerConfig);
            services.AddSingleton<ConsumerConfig>(consumerConfig);

            services.AddHttpClient<DBService.DBServiceClient>(ServiceOptions(configuration, DBServiceBaseAddress));
            services.AddHttpClient<MongoService.MongoServiceClient>(ServiceOptions(configuration, MongoServiceBaseAddress));
            services.AddHttpClient <EventManagerApi.EventManagerApiClient>(ServiceOptions(configuration, EventManagerApiAddress));
            services.AddHttpClient <UtilService.UtilServiceClient>(ServiceOptions(configuration, UtilServiceApiAddress));
            services.AddHttpClient <LobMonthlyBudgetCappingApi.LobMonthlyBudgetCappingApiClient>(ServiceOptions(configuration, LobMonthlyBudgetCappingApiAddress));
            services.AddSingleton<MessageProducer>();
            services.AddSingleton<TransactionMessageProducer<Null, string>>();
            services.AddSingleton<TransactionMessageProducer<string, string>>();
            services.AddSingleton<MessageQueueService>();
            services.AddSingleton<MongoClientConnection>();
            return services;
        }
        static Action<System.Net.Http.HttpClient> ServiceOptions(IConfiguration configuration, string BaseAddress)
        {
            return c =>
            {
                c.BaseAddress = new Uri(configuration[BaseAddress]);
                c.DefaultRequestHeaders.Add("Accept", ApplicationJson);
            };
        }
    }
}
