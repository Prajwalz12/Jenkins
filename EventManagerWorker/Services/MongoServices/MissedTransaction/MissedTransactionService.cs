using Domain.Models;
using Domain.Settings;
using EventManagerWorker.Services.MongoServices;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class MissedTransactionService : IMissedTransactionService
    {

        private readonly IMongoCollection<MissedTransaction> _mongoCollection;

        public MissedTransactionService(IDatabaseSettings settings, MongoClientConnection mongoClientConnection)
        {
            //var client = new MongoClient(settings.ConnectionString);
            var client = mongoClientConnection.client;
            var database = client.GetDatabase(settings.MissedTransactionRequestSettings.DatabaseName);
            _mongoCollection = database.GetCollection<MissedTransaction>(settings.MissedTransactionRequestSettings.CollectionName);
        }
        public List<MissedTransaction> Get() => _mongoCollection.Find(_transaction => true).ToList();

        public MissedTransaction Get(string id) => _mongoCollection.Find<MissedTransaction>(_transaction => _transaction.TransactionId == id).FirstOrDefault();

        public MissedTransaction Create(MissedTransaction transaction) { _mongoCollection.InsertOne(transaction); return transaction; }

        public void Update(string id, MissedTransaction transaction) => _mongoCollection.ReplaceOne(_transaction => _transaction.TransactionId == id, transaction);

        public void Remove(MissedTransaction transaction) => _mongoCollection.DeleteOne(_transaction => _transaction.TransactionId == transaction.TransactionId);

        public void Remove(string id) => _mongoCollection.DeleteOne(_transaction => _transaction.TransactionId == id);

        public List<MissedTransaction> GetByMobileNumber(string mobileNumber) => _mongoCollection.Find<MissedTransaction>(_transaction => _transaction.MobileNumber == mobileNumber).ToList();
        public List<MissedTransaction> GetByTransactionId(string transactionId) => _mongoCollection.Find<MissedTransaction>(_transaction => _transaction.TransactionId == transactionId).ToList();

        public List<MissedTransaction> Get(FilterDefinition<MissedTransaction> filterDefinition) => _mongoCollection.Find(filterDefinition).ToList();

        public void Update(FilterDefinition<MissedTransaction> filterDefinition, UpdateDefinition<MissedTransaction> updateDefinition)
        {
            _mongoCollection.UpdateOne(filterDefinition, updateDefinition);
        }
    }
}
