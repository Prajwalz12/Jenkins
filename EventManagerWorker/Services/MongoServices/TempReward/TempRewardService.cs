using Domain.Settings;
using EventManagerWorker.Services.MongoServices;
using MongoDB.Driver;
using System.Collections.Generic;
using RewardModel = Domain.Models.RewardModel;

namespace Domain.Services
{
    public class TempRewardService : ITempRewardService
    {
        private readonly IMongoCollection<RewardModel.TempReward> _mongoCollection;

        public TempRewardService(IDatabaseSettings settings, MongoClientConnection mongoClientConnection)
        {
            //var client = new MongoClient(settings.ConnectionString);
            var client = mongoClientConnection.client;
            var database = client.GetDatabase(settings.TempRewardSettings.DatabaseName);
            _mongoCollection = database.GetCollection<RewardModel.TempReward>(settings.TempRewardSettings.CollectionName);
        }
        public List<RewardModel.TempReward> Get() => _mongoCollection.Find(_transaction => true).ToList();

        public RewardModel.TempReward Get(string id) => _mongoCollection.Find<RewardModel.TempReward>(_transaction => _transaction.Id == id).FirstOrDefault();

        public RewardModel.TempReward Create(RewardModel.TempReward transaction) { _mongoCollection.InsertOne(transaction); return transaction; }

        public void Update(string id, RewardModel.TempReward transaction) => _mongoCollection.ReplaceOne(_transaction => _transaction.Id == id, transaction);

        public void Remove(RewardModel.TempReward transaction) => _mongoCollection.DeleteOne(_transaction => _transaction.Id == transaction.Id);

        public void Remove(string id) => _mongoCollection.DeleteOne(_transaction => _transaction.Id == id);

        public List<RewardModel.TempReward> GetByMobileNumber(string mobileNumber) => _mongoCollection.Find<RewardModel.TempReward>(_transaction => _transaction.MobileNumber == mobileNumber).ToList();
        public List<RewardModel.TempReward> GetByTransactionId(string transactionId) => _mongoCollection.Find<RewardModel.TempReward>(_transaction => _transaction.Id == transactionId).ToList();

        public List<RewardModel.TempReward> Get(FilterDefinition<RewardModel.TempReward> filter)
        {
            return _mongoCollection.Find(filter).ToList();
        }
    }
}
