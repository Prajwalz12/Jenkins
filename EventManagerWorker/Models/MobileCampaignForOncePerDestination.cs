using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EventManagerWorker.Models
{
    public class MobileCampaignForOncePerDestination
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("id")]
        [JsonProperty("id")]
        public string Id { get; set; } = String.Empty;

        [BsonElement("mobileNumber")]
        [JsonProperty("mobileNumber")]
        public string MobileNumber { get; set; }

        [BsonElement("customerId")]
        [JsonProperty("customerId")]
        public string CustomerId { get; set; }

        [BsonElement("campaignId")]
        [JsonProperty("campaignId")]
        public string CampaignId { get; set; }

        [BsonElement("destinationVPA")]
        [JsonProperty("destinationVPA")]
        public string DestinationVPA { get; set; }

        [BsonElement("billerId")]
        [JsonProperty("billerId")]
        public string BillerId { get; set; }

        [BsonElement("merchantId")]
        [JsonProperty("merchantId")]
        public string MerchantId { get; set; }
    }

    public class MobileCampaignOncePerDestinationRequest
    {

        [BsonElement("mobileNumber")]
        [JsonProperty("mobileNumber")]
        public string MobileNumber { get; set; }

        [BsonElement("customerId")]
        [JsonProperty("customerId")]
        public string CustomerId { get; set; }

        [BsonElement("campaignId")]
        [JsonProperty("campaignId")]
        public string CampaignId { get; set; }

        [BsonElement("destinationVPA")]
        [JsonProperty("destinationVPA")]
        public string DestinationVPA { get; set; }

        [BsonElement("billerId")]
        [JsonProperty("billerId")]
        public string BillerId { get; set; }

        [BsonElement("merchantId")]
        [JsonProperty("merchantId")]
        public string MerchantId { get; set; }
    }
}
