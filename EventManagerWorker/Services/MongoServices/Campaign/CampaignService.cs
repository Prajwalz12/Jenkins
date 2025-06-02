using CampaignModel = Domain.Models.CampaignModel;
using TransactionModel = Domain.Models.TransactionModel;
using Domain.Settings;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver.Linq;
using Utility;
using Domain.Models.CampaignModel;
using System.Linq.Expressions;
using EventManagerWorker.Services.MongoServices;

namespace Domain.Services
{
    public class CampaignService : ICampaignService
    {

        private readonly IMongoCollection<CampaignModel.EarnCampaign> _mongoCollection;
        public CampaignService(IDatabaseSettings settings, MongoClientConnection mongoClientConnection)
        {
            //var client = new MongoClient(settings.ConnectionString);
            var client = mongoClientConnection.client;
            var database = client.GetDatabase(settings.CampaignSettings.DatabaseName);
            _mongoCollection = database.GetCollection<CampaignModel.EarnCampaign>(settings.CampaignSettings.CollectionName);
        }

        public List<CampaignModel.EarnCampaign> Get() => _mongoCollection.Find(_campaign => true).ToList();

        public CampaignModel.EarnCampaign Get(string id) => _mongoCollection.Find<CampaignModel.EarnCampaign>(_campaign => _campaign.Id == id).FirstOrDefault();

        public CampaignModel.EarnCampaign Create(CampaignModel.EarnCampaign campaign) { _mongoCollection.InsertOne(campaign); return campaign; }

        public void Update(string id, CampaignModel.EarnCampaign campaign) => _mongoCollection.ReplaceOne(_campaign => _campaign.Id == id, campaign);

        public void Remove(CampaignModel.EarnCampaign campaign) => _mongoCollection.DeleteOne(_campaign => _campaign.Id == campaign.Id);

        public void Remove(string id) => _mongoCollection.DeleteOne(_campaign => _campaign.Id == id);

        public List<CampaignModel.EarnCampaign> GetCampaignsUsingFilter(Expression<Func<CampaignModel.EarnCampaign, bool>> expression) => _mongoCollection.Find<CampaignModel.EarnCampaign>(Builders<CampaignModel.EarnCampaign>.Filter.Where(expression)).ToList();
        //var query = _mongoCollection.AsQueryable().Where(o => o.General.customer.CampaignDetails.OfferType.ToLower() == GetOfferTypeByEventId(transaction.TransactionRequest.EventId).ToLower());//query.Where(o=> o.IsPublished);//query.Where(o=> o.General.customer.CampaignDetails.StartDate < DateTime.Now && o.General.customer.CampaignDetails.EndDate > DateTime.Now);            //// Date Time Should be From Transaction //return query.ToList();           

        private string GetOfferTypeByEventId(string eventId) => EventManager.GetOfferTypeByEventCode(EventManager.GetEventCodeByEventName(eventId));

        public List<EarnCampaign> GetCampaignsUsingFilter(FilterDefinition<EarnCampaign> filter)
        {
            //var filterD = Builders<EarnCampaign>.Filter.Where(o=> o.Id == "60d17c428f7c43cd93975e1b" );
            var campaigns = _mongoCollection.Find(filter).ToList();
            //var _campaigns = _mongoCollection.Find(_campaign => true).ToList();
            return campaigns;
        }
    }
}