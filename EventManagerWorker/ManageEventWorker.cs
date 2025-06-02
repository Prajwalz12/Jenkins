using Confluent.Kafka;
using Domain.Models.TransactionModel;
using Domain.Processors;
using Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagerWorker
{
    public class ManageEventWorker : BackgroundService
    {
        private readonly ILogger<ManageEventWorker> _logger;

        private readonly Processor _processor;
        private readonly IConfiguration _configuration;
        // private readonly string _topic = "Transactions";
        private readonly IConsumer<string, string> _messageConsumer;

        public ManageEventWorker(IConfiguration configuration, ILogger<ManageEventWorker> logger, Processor processor, ConsumerConfig consumerConfig)
        {
            _logger = logger;
            _processor = processor;
            _configuration = configuration;
            _messageConsumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Transaction Processing Service Started.");
            var isStopProcess = _configuration["IsStopProcess"];
            var bootStrapServers = _configuration["KafkaSettings:BootstrapServers"];
            var transactionTopic = _configuration["KafkaSettings:TransactionTopic"];
            var groupId= _configuration["KafkaSettings:GroupId"];
            _logger.LogInformation($"IsStopProcess {isStopProcess} ");
            if (!String.IsNullOrEmpty(isStopProcess) && "No".Equals(isStopProcess))
            {
                string topic = transactionTopic;
                var conf = new ConsumerConfig
                {
                    GroupId = groupId,
                    BootstrapServers = bootStrapServers,
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = false
                };
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    using (var builder = new ConsumerBuilder<Ignore, string>(conf).Build())
                    {
                        builder.Subscribe(topic);
                        var cancelToken = new CancellationTokenSource();
                        try
                        {
                            while (true)
                            {
                                var cr = builder.Consume(cancelToken.Token);
                                var message = cr.Message.Value;

                                _logger.LogInformation($"Receive Offset : {cr.Offset}");
                                _logger.LogInformation($"{JsonConvert.SerializeObject(new { ID = cr.Offset, Message = cr.Message, Partition = cr.TopicPartition.Partition.Value })}");
                                try
                                {
                                    await StartConsumerLoop(message).ConfigureAwait(false);
                                    await Task.Delay(100).ConfigureAwait(false);
                                    builder.Commit(cr);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, $"{ex.Message}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"{ex.Message}");
                        }
                        finally
                        {
                            builder.Close();
                        }
                    }
                }
            }
        }

        private async Task StartConsumerLoop(string message)
        {
            var transactionRequest = message.GetResult<Domain.Models.TransactionModel.ProcessedTransaction>();
            await _processor.Process(new List<Domain.Models.TransactionModel.ProcessedTransaction>() { transactionRequest });
        }

        public override void Dispose()
        {
            this._messageConsumer.Close(); // Commit offsets and leave the group cleanly.
            this._messageConsumer.Dispose();

            base.Dispose();
        }

    }
}
