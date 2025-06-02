using Domain.Settings;
using EventManagerWorker.Services.MongoServices;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using System.Linq;
using CustomerModel = Domain.Models.CustomerModel;

namespace Domain.Services
{
    public class CustomerVersionService : ICustomerVersionService
    {
        private readonly IMongoCollection<CustomerModel.CustomerVersion> _mongoCollection;

        public CustomerVersionService(IDatabaseSettings settings, MongoClientConnection mongoClientConnection)
        {
            //var client = new MongoClient(settings.ConnectionString);
            var client = mongoClientConnection.client;
            var database = client.GetDatabase(settings.CustomerVersionSettings.DatabaseName);
            _mongoCollection = database.GetCollection<CustomerModel.CustomerVersion>(settings.CustomerVersionSettings.CollectionName);
        }

        public List<CustomerModel.CustomerVersion> Get() => _mongoCollection.Find(_customerVersion => true).ToList();

        public CustomerModel.CustomerVersion Get(string id) => _mongoCollection.Find<CustomerModel.CustomerVersion>(_customerVersion => _customerVersion.CustomerVersionId == id).FirstOrDefault();

        public CustomerModel.CustomerVersion Create(CustomerModel.CustomerVersion customerVersion) { _mongoCollection.InsertOne(customerVersion); return customerVersion; }

        //public void Update(string id, CustomerVersion customerVersion) => _mongoCollection.ReplaceOne(_customerVersion => _customerVersion.CustomerVersionId == id, customerVersion);

        //public void Remove(CustomerVersion customerVersion) => _mongoCollection.DeleteOne(_customerVersion => _customerVersion.CustomerVersionId == customerVersion.CustomerVersionId);

        //public void Remove(string id) => _mongoCollection.DeleteOne(_customerVersion => _customerVersion.CustomerVersionId == id);

        public CustomerModel.CustomerVersion GetByMobileNumber(string mobileNumber) => _mongoCollection.Find<CustomerModel.CustomerVersion>(_customer => _customer.MobileNumber == mobileNumber).FirstOrDefault();
        public CustomerModel.CustomerVersion GetLatestCustomerVersion(string mobileNumber) => _mongoCollection.AsQueryable().Where(cv => cv.MobileNumber == mobileNumber).AsQueryable().OrderByDescending(l => l.CustomerVersionId).FirstOrDefault();
    }
}
