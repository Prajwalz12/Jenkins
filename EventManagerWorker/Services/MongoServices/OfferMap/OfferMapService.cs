using Domain.Models.TransactionModel;
using Domain.Settings;
using EventManagerWorker.Services.MongoServices;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Services
{
    public class OfferMapService : IOfferMapService
    {
        private readonly IMongoCollection<ProcessedTransaction> _mongoCollection;

        public OfferMapService(IDatabaseSettings settings, MongoClientConnection mongoClientConnection)
        {
            //var client = new MongoClient(settings.ConnectionString);
            var client = mongoClientConnection.client;
            var database = client.GetDatabase(settings.OfferMapSettings.DatabaseName);
            _mongoCollection = database.GetCollection<ProcessedTransaction>(settings.OfferMapSettings.CollectionName);
        }
        public List<ProcessedTransaction> Get() => _mongoCollection.Find(_transaction => true).ToList();

        public ProcessedTransaction Get(string id) => _mongoCollection.Find<ProcessedTransaction>(_transaction => _transaction.TransactionRequest.TransactionId == id).FirstOrDefault();

        public ProcessedTransaction Create(ProcessedTransaction transaction) { _mongoCollection.InsertOne(transaction); return transaction; }

        public void Update(string id, ProcessedTransaction transaction) => _mongoCollection.ReplaceOne(_transaction => _transaction.TransactionRequest.TransactionId == id, transaction);

        public void Remove(ProcessedTransaction transaction) => _mongoCollection.DeleteOne(_transaction => _transaction.TransactionRequest.TransactionId == transaction.TransactionRequest.TransactionId);

        public void Remove(string id) => _mongoCollection.DeleteOne(_transaction => _transaction.TransactionRequest.TransactionId == id);

        public List<ProcessedTransaction> GetByMobileNumber(string mobileNumber) => _mongoCollection.Find<ProcessedTransaction>(_transaction => _transaction.TransactionRequest.MobileNumber == mobileNumber).ToList();
        public List<ProcessedTransaction> GetByTransactionId(string transactionId) => _mongoCollection.Find<ProcessedTransaction>(_transaction => _transaction.TransactionRequest.TransactionId == transactionId).ToList();
    }
}
