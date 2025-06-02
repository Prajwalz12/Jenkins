using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models
{
    public class GroupedCampaignTransaction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("id")]
        [JsonProperty("id")]
        public string Id { get; set; } = null;

        [BsonElement("transactionId")]
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; } = null;

        [BsonElement("transactionReferenceNumber")]
        [JsonProperty("transactionReferenceNumber")]
        public string TransactionReferenceNumber { get; set; } = null;

        [BsonElement("groupedCampaignId")]
        [JsonProperty("groupedCampaignId")]
        public string GroupedCampaignId { get; set; } = null;

        [BsonElement("campaignId")]
        [JsonProperty("campaignId")]
        public string CampaignId { get; set; } = null;
    }
}
