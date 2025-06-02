using Domain.Settings;
using EventManagerWorker.Services.MongoServices;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using CustomerModel = Domain.Models.CustomerModel;

namespace Domain.Services
{
    public class CustomerEventService : ICustomerEventService
    {
        private readonly IMongoCollection<CustomerModel.CustomerEvent> _mongoCollection;

       public CustomerEventService(IDatabaseSettings settings, MongoClientConnection mongoClientConnection)
        {
            //var client = new MongoClient(settings.ConnectionString);
            var client = mongoClientConnection.client;
            var database = client.GetDatabase(settings.CustomerEventSettings.DatabaseName);
            _mongoCollection = database.GetCollection<CustomerModel.CustomerEvent>(settings.CustomerEventSettings.CollectionName);
        }
        //public CustomerEventService(CustomerSettings customerSettings)
        //{
        //    var client = new MongoClient(settings.ConnectionString);
        //    var database = client.GetDatabase(customerSettings.DatabaseName);
        //    _mongoCollection = database.GetCollection<CustomerModel.CustomerEvent>(customerSettings.CollectionName);
        //}

        public List<CustomerModel.CustomerEvent> Get() => _mongoCollection.Find(_customer => true).ToList();

        public CustomerModel.CustomerEvent Get(string id) => _mongoCollection.Find<CustomerModel.CustomerEvent>(_customer => _customer.LoyaltyId == id).FirstOrDefault();

        public CustomerModel.CustomerEvent Create(CustomerModel.CustomerEvent customer) { _mongoCollection.InsertOne(customer); return customer; }

        public void Update(string id, CustomerModel.CustomerEvent customer) => _mongoCollection.ReplaceOne(_customer => _customer.Id == id, customer);

        public void Remove(CustomerModel.CustomerEvent customer) => _mongoCollection.DeleteOne(_customer => _customer.LoyaltyId == customer.LoyaltyId);

        public void Remove(string id) => _mongoCollection.DeleteOne(_customer => _customer.Id == id);

        public CustomerModel.CustomerEvent GetByMobileNumber(string mobileNumber) => _mongoCollection.Find<CustomerModel.CustomerEvent>(_customer => _customer.MobileNumber == mobileNumber).FirstOrDefault();

        public List<CustomerModel.CustomerEvent> GetCustomerEvents(FilterDefinition<CustomerModel.CustomerEvent> filter)
        {            
            return _mongoCollection.Find<CustomerModel.CustomerEvent>(filter).ToList();
        }
        public long GetCustomerEventsCount(FilterDefinition<CustomerModel.CustomerEvent> filter)
        {
            return _mongoCollection.CountDocuments(filter);
        }
    }
}
