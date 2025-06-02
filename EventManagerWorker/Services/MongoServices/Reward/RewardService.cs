using Domain.Models.RewardModel;
using Domain.Settings;
using EventManagerWorker.Services.MongoServices;
using MongoDB.Driver;
using System.Collections.Generic;
using RewardModel = Domain.Models.RewardModel;

namespace Domain.Services
{
    public class RewardService : IRewardService
    {
        private readonly IMongoCollection<RewardModel.TransactionReward> _mongoCollection;
        private readonly IMongoDatabase _mongoDatabase;

        public RewardService(IDatabaseSettings settings, MongoClientConnection mongoClientConnection)
        {
            //var client = new MongoClient(settings.ConnectionString);
            var client = mongoClientConnection.client;
            _mongoDatabase = client.GetDatabase(settings.TransactionRewardSettings.DatabaseName);
            _mongoCollection = _mongoDatabase.GetCollection<RewardModel.TransactionReward>(settings.TransactionRewardSettings.CollectionName);
        }
        private IMongoCollection<T> GetMongoCollection<T>(string collectionName)
        {
           return _mongoDatabase.GetCollection<T>(collectionName);
        }       

        public List<TransactionReward> Get(string collectionName, FilterDefinition<TransactionReward> filter)
        {
            return GetMongoCollection<TransactionReward>(collectionName).Find(filter).ToList();
        }    
        
        public List<TempReward> GetTempRewards(string collectionName, FilterDefinition<TempReward> filter)
        {
            return GetMongoCollection<TempReward>(collectionName).Find(filter).ToList();
        }
    }
}
