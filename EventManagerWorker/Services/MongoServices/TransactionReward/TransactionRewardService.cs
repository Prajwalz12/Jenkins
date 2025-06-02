using Domain.Settings;
using EventManagerWorker.Services.MongoServices;
using MongoDB.Driver;
using System.Collections.Generic;
using RewardModel = Domain.Models.RewardModel;

namespace Domain.Services
{
    public class TransactionRewardService : ITransactionRewardService
    {
        private readonly IMongoCollection<RewardModel.TransactionReward> _mongoCollection;

        public TransactionRewardService(IDatabaseSettings settings, MongoClientConnection mongoClientConnection)
        {
            //var client = new MongoClient(settings.ConnectionString);
            var client = mongoClientConnection.client;
            var database = client.GetDatabase(settings.TransactionRewardSettings.DatabaseName);
            _mongoCollection = database.GetCollection<RewardModel.TransactionReward>(settings.TransactionRewardSettings.CollectionName);
        }
        public List<RewardModel.TransactionReward> Get() => _mongoCollection.Find(_transaction => true).ToList();

        public RewardModel.TransactionReward Get(string id) => _mongoCollection.Find<RewardModel.TransactionReward>(_transaction => _transaction.Id == id).FirstOrDefault();

        public RewardModel.TransactionReward Create(RewardModel.TransactionReward transaction) { _mongoCollection.InsertOne(transaction); return transaction; }

        public void Update(string id, RewardModel.TransactionReward transaction) => _mongoCollection.ReplaceOne(_transaction => _transaction.Id == id, transaction);

        public void Remove(RewardModel.TransactionReward transaction) => _mongoCollection.DeleteOne(_transaction => _transaction.Id == transaction.Id);

        public void Remove(string id) => _mongoCollection.DeleteOne(_transaction => _transaction.Id == id);

        public List<RewardModel.TransactionReward> GetByMobileNumber(string mobileNumber) => _mongoCollection.Find<RewardModel.TransactionReward>(_transaction => _transaction.MobileNumber == mobileNumber).ToList();
        public List<RewardModel.TransactionReward> GetByTransactionId(string transactionId) => _mongoCollection.Find<RewardModel.TransactionReward>(_transaction => _transaction.Id == transactionId).ToList();

        public List<RewardModel.TransactionReward> Get(FilterDefinition<RewardModel.TransactionReward> filter)
        {
            return _mongoCollection.Find(filter).ToList();
        }
    }
}
