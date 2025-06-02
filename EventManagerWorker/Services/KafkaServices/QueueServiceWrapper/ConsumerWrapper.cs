namespace Domain.Services.QueueServiceWrapper
{
    using Confluent.Kafka;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    public class ConsumerWrapper
    {
        private readonly string _topicName;
        private readonly ConsumerConfig _consumerConfig;
        private static IConsumer<string, string> _consumer;
        public ConsumerWrapper(ConsumerConfig config, string topicName)
        {
            _topicName = topicName;
            _consumerConfig = config;
            _consumer = new ConsumerBuilder<string, string>(_consumerConfig).Build();
            _consumer.Subscribe(topicName);
        }        
        public string ReadMessage()
        {
            var consumeResult = _consumer.Consume();
            return consumeResult.Message.Value;
        }
        
    }
}