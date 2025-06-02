using DBService;
using Domain.Settings;
using EventManagerWorker.Models;
using EventManagerWorker.Services.MongoServices;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Domain.Services
{
    public class MerchantMasterService : IMerchantMaster
    {
        private readonly IMongoCollection<MerchantMaster> _mongoCollection;
        public MerchantMasterService(IDatabaseSettings settings, MongoClientConnection mongoClientConnection)
        {
            var client = mongoClientConnection.client;
            var database = client.GetDatabase(settings.MerchantMasterSettings.DatabaseName);
            _mongoCollection = database.GetCollection<MerchantMaster>(settings.MerchantMasterSettings.CollectionName);
        }

        MerchantMaster IMerchantMaster.GetMerchantMasterValues(MerchantEnumRequest request)
        {
            var builder = Builders<MerchantMaster>.Filter;
            FilterDefinition<MerchantMaster> filter = null;

            if (!string.IsNullOrEmpty(request.Source) && request.Source == "finnone")
            {
                filter = builder.Where(r => r.FinnoneDealerCode == request.MerchantId && r.isNonMerchant == 0);
            }
            else
            {
                filter = builder.Where(r => r.MerchantID == request.MerchantId && r.isNonMerchant == 0);
            }
            if (!string.IsNullOrEmpty(request.Category))
            {
                filter &= builder.Where(r => r.MerchantCategory == request.Category);
            }
            if (request.TripleReward != null)
            {
                filter &= builder.Where(r => r.TripleReward == request.TripleReward);
            }
            if (!string.IsNullOrEmpty(request.MerchantType))
            {
                filter &= builder.Where(r => r.MType == request.MerchantType);
            }

            var merchantValues = _mongoCollection.Find<MerchantMaster>(filter).FirstOrDefault();

            if (merchantValues != null)
            {
                return merchantValues;
            }
            else
            {
                return null;
            }
        }
    }
}
