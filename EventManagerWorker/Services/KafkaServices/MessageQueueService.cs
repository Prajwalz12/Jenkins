using Confluent.Kafka;
using Domain.Services.Kafka;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Services.Kafka
{
    public class MessageQueueService
    {
        private readonly ILogger<MessageQueueService> _logger;
        private readonly TransactionMessageProducer<string, string> _transactionMessageProducer;
        public MessageQueueService(TransactionMessageProducer<string, string> transactionMessageProducer, ILogger<MessageQueueService> logger)
        {
            _transactionMessageProducer = transactionMessageProducer;
            _logger = logger;
        }
        public async Task ProduceTransactionAsync(string topic, string key, string message)
        {
            await _transactionMessageProducer.ProduceAsync(topic, new Message<string, string>() { Key = key, Value = message}).ConfigureAwait(false);
        }
        public void ProduceTransaction(string topic, string key, string message)
        {
            _transactionMessageProducer.Produce(topic, new Message<string, string>() { Key = key, Value = message}, DeliveryReportHandler);
        }
        private void DeliveryReportHandler(DeliveryReport<string, string> deliveryReport)
        {
            if (deliveryReport.Status == PersistenceStatus.NotPersisted)
            {
                // It is common to write application logs to Kafka (note: this project does not provide
                // an example logger implementation that does this). Such an implementation should
                // ideally fall back to logging messages locally in the case of delivery problems.
                this._logger.LogInformation($"Message delivery failed: {deliveryReport.Message.Key} {deliveryReport.Message.Value}");
            }
        }
    }
}
