using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services.QueueServiceWrapper
{
    public class ProducerWrapper
    {
        private string _topicName;
        private IProducer<string, string> _producer;
        private ProducerConfig _config;        

        public ProducerWrapper(ProducerConfig config, string topicName)
        {
            this._topicName = topicName;
            this._config = config;
            this._producer = new ProducerBuilder<string, string>(_config).Build();            
        }
        public async Task WriteMessageAsync(string message)
        {
            _ = await this._producer.ProduceAsync(this._topicName, new Message<string, string>()
            {
                Key = Guid.NewGuid().ToString(),
                Value = message
            });
        }
        
        public void WriteMessage(string message)
        {
            _producer.Produce(_topicName, new Message<string, string>()
            {
                Key = Guid.NewGuid().ToString(),
                Value = message
            });           
        }
    }
}
