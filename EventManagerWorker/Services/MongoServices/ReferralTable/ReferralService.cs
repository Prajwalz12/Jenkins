using Domain.Settings;
using EventManagerWorker.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManagerWorker.Services.MongoServices.ReferralTable
{
    internal class ReferralService:IReferralService
    {
        private readonly IMongoCollection<Referral> _mongoCollection;

        public ReferralService(IDatabaseSettings settings, MongoClientConnection mongoClientConnection)
        {
            //var client = new MongoClient(settings.ConnectionString);
            var client = mongoClientConnection.client;
            var database = client.GetDatabase(settings.ReferraTransactionlSettings.DatabaseName);
            _mongoCollection = database.GetCollection<Referral>(settings.ReferraTransactionlSettings.CollectionName);
        }

        public List<Referral> Get() => _mongoCollection.Find(_referral => true).ToList();

        public Referral Get(string id) => _mongoCollection.Find<Referral>(_referral => _referral.ReferralId == id).FirstOrDefault();

        public Referral Create(Referral referral) { _mongoCollection.InsertOne(referral); return referral; }

        public void Update(string id, Referral transaction) => _mongoCollection.ReplaceOne(_referral => _referral.ReferralId == id, transaction);

        public void Remove(Referral referral) => _mongoCollection.DeleteOne(_referral => _referral.ReferralId == referral.ReferralId);

        public void Remove(string id) => _mongoCollection.DeleteOne(_referral => _referral.ReferralId == id);

        public List<Referral> GetByMobileNumber(string mobileNumber) => _mongoCollection.Find<Referral>(_referral => _referral.ReferrerMobNumber == mobileNumber).ToList();
        public List<Referral> GetByTransactionId(string referralId) => _mongoCollection.Find<Referral>(_referral => _referral.ReferralId == _referral.ReferralId).ToList();

        public List<Referral> Get(FilterDefinition<Referral> filterDefinition) => _mongoCollection.Find(filterDefinition).ToList();

    }
}
