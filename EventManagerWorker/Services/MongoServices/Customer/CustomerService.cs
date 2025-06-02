using Domain.Models.Common;
using Domain.Settings;
using EventManagerWorker.Services.MongoServices;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomerModel = Domain.Models.CustomerModel;

namespace Domain.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IMongoCollection<CustomerModel.Customer> _mongoCollection;

        public CustomerService(IDatabaseSettings settings, MongoClientConnection mongoClientConnection)
        {
            //var client = new MongoClient(settings.ConnectionString);
            var client = mongoClientConnection.client;
            var database = client.GetDatabase(settings.CustomerSettings.DatabaseName);
            _mongoCollection = database.GetCollection<CustomerModel.Customer>(settings.CustomerSettings.CollectionName);
        }

        public List<CustomerModel.Customer> Get() => _mongoCollection.Find(_customer => true).ToList();

        public CustomerModel.Customer Get(string id) => _mongoCollection.Find<CustomerModel.Customer>(_customer => _customer.LoyaltyId == id).FirstOrDefault();

        public CustomerModel.Customer Create(CustomerModel.Customer customer) { _mongoCollection.InsertOne(customer); return customer; }

        public void Update(string id, CustomerModel.Customer customer) => _mongoCollection.ReplaceOne(_customer => _customer.LoyaltyId == id, customer);

        public void Remove(CustomerModel.Customer customer) => _mongoCollection.DeleteOne(_customer => _customer.LoyaltyId == customer.LoyaltyId);

        public void Remove(string id) => _mongoCollection.DeleteOne(_customer => _customer.LoyaltyId == id);

        public CustomerModel.Customer GetByMobileNumber(string mobileNumber) => _mongoCollection.Find<CustomerModel.Customer>(_customer => _customer.MobileNumber == mobileNumber).FirstOrDefault();

       // public CustomerModel.Customer GetByMobileNumberAsync(string mobileNumber) => _mongoCollection.Find<CustomerModel.Customer>(_customer => _customer.MobileNumber == mobileNumber).FirstOrDefault();

        public async Task<CustomerModel.Customer> GetByMobileNumberAsync(string mobileNumber)
        {
            var _cursor = await _mongoCollection.FindAsync<CustomerModel.Customer>(_customer => _customer.MobileNumber == mobileNumber);
            return await _cursor.FirstOrDefaultAsync().ConfigureAwait(false);
        }
        public UpdateResult Update(string loyaltyId, Wallet wallet)
        {
            var filter = Builders<CustomerModel.Customer>.Filter.Where(o=> o.LoyaltyId == loyaltyId);
            var updateQuery = Builders<CustomerModel.Customer>.Update.Set(o=> o.Wallet.Id, wallet.Id).Set(o=> o.Flags.Wallet, true);
            return _mongoCollection.UpdateOne(filter, updateQuery);
        }
    }
}
