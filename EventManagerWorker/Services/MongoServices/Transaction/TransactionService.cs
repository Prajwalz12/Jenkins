using Domain.Models.TransactionModel;
using Domain.Settings;
using EventManagerWorker.Services.MongoServices;
using MongoDB.Driver;
using System.Collections.Generic;
using TransactionModel = Domain.Models.TransactionModel;

namespace Domain.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IMongoCollection<TransactionModel.Transaction> _mongoCollection;

        public TransactionService(IDatabaseSettings settings, MongoClientConnection mongoClientConnection)
        {
            //var client = new MongoClient(settings.ConnectionString);
            var client = mongoClientConnection.client;
            var database = client.GetDatabase(settings.TransactionSettings.DatabaseName);
            _mongoCollection = database.GetCollection<TransactionModel.Transaction>(settings.TransactionSettings.CollectionName);
        }
        public List<TransactionModel.Transaction> Get() => _mongoCollection.Find(_transaction => true).ToList();

        public TransactionModel.Transaction Get(string id) => _mongoCollection.Find<TransactionModel.Transaction>(_transaction => _transaction.TransactionId == id).FirstOrDefault();

        public TransactionModel.Transaction Create(TransactionModel.Transaction transaction) { _mongoCollection.InsertOne(transaction); return transaction; }

        public void Update(string id, TransactionModel.Transaction transaction) => _mongoCollection.ReplaceOne(_transaction => _transaction.TransactionId == id, transaction);

        public void Remove(TransactionModel.Transaction transaction) => _mongoCollection.DeleteOne(_transaction => _transaction.TransactionId == transaction.TransactionId);

        public void Remove(string id) => _mongoCollection.DeleteOne(_transaction => _transaction.TransactionId == id);

        public List<TransactionModel.Transaction> GetByMobileNumber(string mobileNumber) => _mongoCollection.Find<TransactionModel.Transaction>(_transaction => _transaction.MobileNumber == mobileNumber).ToList();
        public List<TransactionModel.Transaction> GetByTransactionId(string transactionId) => _mongoCollection.Find<TransactionModel.Transaction>(_transaction => _transaction.TransactionId == transactionId).ToList();

        public List<Transaction> Get(FilterDefinition<Transaction> filterDefinition) => _mongoCollection.Find(filterDefinition).ToList();
    }
}
