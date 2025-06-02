using Confluent.Kafka;
using System;
using System.Threading.Tasks;

namespace Domain.Services.Kafka
{
    public class MessageProducer : IDisposable
    {
        IProducer<byte[], byte[]> kafkaProducer;

        public MessageProducer(ProducerConfig producerConfig)
        {
            //var conf = new ProducerConfig();
            //config.GetSection("Kafka:ProducerSettings").Bind(conf);
            this.kafkaProducer = new ProducerBuilder<byte[], byte[]>(producerConfig).Build();
        }

        public Handle Handle { get => this.kafkaProducer.Handle; }

        public void Dispose()
        {
            // Block until all outstanding produce requests have completed (with or
            // without error).
            this.kafkaProducer.Flush();
            this.kafkaProducer.Dispose();
        }
    }
    public class TransactionMessageProducer<K, V>
    {
        IProducer<K, V> _dependentProducer;

        public TransactionMessageProducer(MessageProducer producer)
        {
            _dependentProducer = new DependentProducerBuilder<K, V>(producer.Handle).Build();
        }

        public Task ProduceAsync(string topic, Message<K, V> message) => this._dependentProducer.ProduceAsync(topic, message);

        public void Produce(string topic, Message<K, V> message, Action<DeliveryReport<K, V>> deliveryHandler = null) => this._dependentProducer.Produce(topic, message, deliveryHandler);

        public void Flush(TimeSpan timeout) => this._dependentProducer.Flush(timeout);
    }
    //public class ConsumerProducer<K, V>
    //{
    //    IProducer<K, V> _dependentProducer;

    //    public ConsumerProducer(Producer producer)
    //    {
    //        _dependentProducer = new DependentProducerBuilder<K, V>(producer.Handle).Build();
    //    }

    //    public Task ProduceAsync(string topic, Message<K, V> message) => this._dependentProducer.ProduceAsync(topic, message);

    //    public void Produce(string topic, Message<K, V> message, Action<DeliveryReport<K, V>> deliveryHandler = null) => this._dependentProducer.Produce(topic, message, deliveryHandler);

    //    public void Flush(TimeSpan timeout) => this._dependentProducer.Flush(timeout);
    //}
}
