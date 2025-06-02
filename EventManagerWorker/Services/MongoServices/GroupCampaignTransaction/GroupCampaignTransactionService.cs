using Domain.Models;
using Domain.Settings;
using EventManagerWorker.Services.MongoServices;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Services
{
    public class GroupCampaignTransactionService : IGroupCampaignTransactionService
    {
        private readonly IMongoCollection<GroupedCampaignTransaction> _mongoCollection;
        public GroupCampaignTransactionService(IDatabaseSettings settings, MongoClientConnection mongoClientConnection)
        {
            //var client = new MongoClient(settings.ConnectionString);
            var client = mongoClientConnection.client;
            var database = client.GetDatabase(settings.TransactionSettings.DatabaseName);
            _mongoCollection = database.GetCollection<GroupedCampaignTransaction>(settings.GroupCampaignTransactionSettings.CollectionName);
        }

        public List<GroupedCampaignTransaction> Get() => _mongoCollection.Find(_transaction => true).ToList();

        public GroupedCampaignTransaction Get(string id) => _mongoCollection.Find<GroupedCampaignTransaction>(_transaction => _transaction.TransactionId == id).FirstOrDefault();

        public GroupedCampaignTransaction Create(GroupedCampaignTransaction transaction) { _mongoCollection.InsertOne(transaction); return transaction; }

        public void Update(string id, GroupedCampaignTransaction transaction) => _mongoCollection.ReplaceOne(_transaction => _transaction.TransactionId == id, transaction);

        public void Remove(GroupedCampaignTransaction transaction) => _mongoCollection.DeleteOne(_transaction => _transaction.TransactionId == transaction.TransactionId);

        public void Remove(string id) => _mongoCollection.DeleteOne(_transaction => _transaction.TransactionId == id);

        public List<GroupedCampaignTransaction> GetByTransactionParentTransactionId(string parentTransactionId) => _mongoCollection.Find<GroupedCampaignTransaction>(_transaction => _transaction.TransactionId == parentTransactionId).ToList();

        public List<GroupedCampaignTransaction> Get(FilterDefinition<GroupedCampaignTransaction> filterDefinition) => _mongoCollection.Find(filterDefinition).ToList();
    }
}
