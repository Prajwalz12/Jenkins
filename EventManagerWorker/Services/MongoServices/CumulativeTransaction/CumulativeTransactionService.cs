using TransactionModel = Domain.Models.TransactionModel;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using Domain.Settings;
using EventManagerWorker.Services.MongoServices;

namespace Domain.Services
{
    public class CumulativeTransactionService : ICumulativeTransactionService
    {
        private readonly IMongoCollection<TransactionModel.ProcessedTransaction> _mongoCollection;
        public CumulativeTransactionService(IDatabaseSettings settings, MongoClientConnection mongoClientConnection)
        {
            //var client = new MongoClient(settings.ConnectionString);
            var client = mongoClientConnection.client;
            var database = client.GetDatabase(settings.CumulativeTransactionSettings.DatabaseName);
            _mongoCollection = database.GetCollection<TransactionModel.ProcessedTransaction>(settings.CumulativeTransactionSettings.CollectionName);
        }
        public List<TransactionModel.ProcessedTransaction> Get() => _mongoCollection.Find(_transaction => true).ToList();

        public TransactionModel.ProcessedTransaction Get(string id) => _mongoCollection.Find<TransactionModel.ProcessedTransaction>(_transaction => _transaction.TransactionRequest.TransactionId == id).FirstOrDefault();

        public TransactionModel.ProcessedTransaction Create(TransactionModel.ProcessedTransaction transaction) { _mongoCollection.InsertOne(transaction); return transaction; }public void Update(string id, TransactionModel.ProcessedTransaction transaction) => _mongoCollection.ReplaceOne(_transaction => _transaction.TransactionRequest.TransactionId == id, transaction);

        public void Remove(TransactionModel.ProcessedTransaction transaction) => _mongoCollection.DeleteOne(_transaction => _transaction.TransactionRequest.TransactionId == transaction.TransactionRequest.TransactionId);

        public void Remove(string id) => _mongoCollection.DeleteOne(_transaction => _transaction.TransactionRequest.TransactionId == id);

        public List<TransactionModel.ProcessedTransaction> GetByMobileNumber(string mobileNumber) => _mongoCollection.Find<TransactionModel.ProcessedTransaction>(_transaction => _transaction.TransactionRequest.MobileNumber == mobileNumber).ToList();
        public List<TransactionModel.ProcessedTransaction> GetByTransactionId(string transactionId) => _mongoCollection.Find<TransactionModel.ProcessedTransaction>(_transaction => _transaction.TransactionRequest.TransactionId == transactionId).ToList();
    }
}
