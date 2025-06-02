using Domain.Builders;
using Domain.Settings;
using EventManagerWorker.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagerWorker.Services.MongoServices.OncePerDestination
{
    public class OncePerDestinationVpaIdService : IOncePerDestionVpaId
    {
        private readonly IMongoCollection<MobileCampaignForOncePerDestination> _mongoCollection;
        public OncePerDestinationVpaIdService(IDatabaseSettings settings, MongoClientConnection mongoClientConnection)
        {
            var client = mongoClientConnection.client;
            var database = client.GetDatabase(settings.MobileCampaignOncePerDestinationSettings.DatabaseName);
            _mongoCollection = database.GetCollection<MobileCampaignForOncePerDestination>(settings.MobileCampaignOncePerDestinationSettings.CollectionName);
        }

        public MobileCampaignForOncePerDestination GetMobileCampaignDestination(MobileCampaignOncePerDestinationRequest request)
        {
            var filterDefinition = QueryBuilder.PrepareFilterQuery(request);
            var result = _mongoCollection.Find<MobileCampaignForOncePerDestination>(filterDefinition).FirstOrDefault(); ;
            if (result != null)
            {
                return result;
            }
            else
            {
                return new MobileCampaignForOncePerDestination();
            }
        }

        public  long GetTransactionCountForOncePerPayee(string mobileNumber, string campId)
        {
            var totalCount = _mongoCollection.CountDocuments<MobileCampaignForOncePerDestination>(x => x.MobileNumber == mobileNumber && x.CampaignId == campId);
            return totalCount;
        }
    }
}
