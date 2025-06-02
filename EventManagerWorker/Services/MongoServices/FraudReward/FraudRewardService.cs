using Domain.Models.RewardModel;
using Domain.Services;
using Domain.Settings;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventManagerWorker.Services.MongoServices;

namespace Domain.Services
{
    public class FraudRewardService : IFraudRewardService
    {
        private readonly IMongoCollection<Domain.Models.RewardModel.FraudReward> _mongoCollection;
        public FraudRewardService(IDatabaseSettings settings, MongoClientConnection mongoClientConnection)
        {
            var client = mongoClientConnection.client;
            var database = client.GetDatabase(settings.FraudRewardSettings.DatabaseName);
            _mongoCollection = database.GetCollection<Domain.Models.RewardModel.FraudReward>(settings.FraudRewardSettings.CollectionName);
        }

        public List<Domain.Models.RewardModel.FraudReward> Get(FilterDefinition<Domain.Models.RewardModel.FraudReward> filter)
        {
            return _mongoCollection.Find(filter).ToList();
        }
    }
}
