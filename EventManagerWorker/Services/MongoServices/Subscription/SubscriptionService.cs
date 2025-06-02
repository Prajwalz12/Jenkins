using Domain.Settings;
using EventManagerWorker.Services.MongoServices;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubscriptionModel = Domain.Models.SubscriptionModel;

namespace Domain.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ILogger<SubscriptionService> _logger;
        private readonly IMongoCollection<SubscriptionModel.Subscription> _mongoCollection;

        public SubscriptionService(ILogger<SubscriptionService> logger, IDatabaseSettings settings, MongoClientConnection mongoClientConnection)
        {
            _logger = logger;
            var client = mongoClientConnection.client;
            var database = client.GetDatabase(settings.SubscriptionSettings.DatabaseName);
            _mongoCollection = database.GetCollection<SubscriptionModel.Subscription>(settings.SubscriptionSettings.CollectionName);
        }

        public async Task<SubscriptionModel.Subscription> GetSubscriptionAsync(string subscriptionId)
        {
            _logger.LogInformation("Calling GetSubscriptionAsync() with the parameter : {subscriptionId}", subscriptionId);
            return await _mongoCollection.Find(s => s.Id == subscriptionId).FirstOrDefaultAsync().ConfigureAwait(false);
        }
        public SubscriptionModel.Subscription GetSubscriptionById(string preFix, string subscriptionId)
        {
            _logger.LogInformation("{preFix} Calling GetSubscriptionAsync() with the parameter : {subscriptionId}", preFix, subscriptionId);
            return _mongoCollection.Find(s => s.Id == subscriptionId).FirstOrDefault();
        }

        public async Task<List<SubscriptionModel.Subscription>> GetEligibleSubscriptionsAsync()
        {
            var query = Builders<Domain.Models.SubscriptionModel.Subscription>.Filter;
            FilterDefinition<Domain.Models.SubscriptionModel.Subscription> filter = query.Lte(q => q.StartDateTime, DateTime.Now) & (query.Gte(q => q.EndDateTime, DateTime.Now) | query.Eq(q => q.Type, "Trial"));
            var result = await _mongoCollection.FindAsync(filter).ConfigureAwait(false);
            return await result.ToListAsync().ConfigureAwait(false);
        }
    }
}