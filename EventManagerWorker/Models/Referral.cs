using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Core.Tokens;

namespace EventManagerWorker.Models
{
    [BsonIgnoreExtraElements]
    public class Referral
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("referralId")]
        [JsonProperty("referralId")]
        public string ReferralId { get; set; }
        [BsonElement("referrerMobNumber")]
        [JsonProperty("referrerMobNumber")]
        public string ReferrerMobNumber { get; set; }
        [BsonElement("refereeMobNumber")]
        [JsonProperty("refereeMobNumber")]
        public string RefereeMobNumber { get; set; }
        [BsonElement("campaignID")]
        [JsonProperty("campaignID")]
        public string CampaignID { get; set; }
        [BsonElement("date")]
        [JsonProperty("date")]
        public DateTime Date { get; set; }
        [BsonElement("rewardType")]
        [JsonProperty("rewardType")]
        public string RewardType { get; set; }
        [BsonElement("rewardValue")]
        [JsonProperty("rewardValue")]
        public string RewardValue { get; set; }
        [BsonElement("direct")]
        [JsonProperty("direct")]
        public bool Direct { get; set; }
        [BsonElement("lock")]
        [JsonProperty("lock")]
        public bool Lock { get; set; }
        [BsonElement("eventId")]
        [JsonProperty("eventId")]
        public string EventId { get; set; }
        [BsonElement("lob")]
        [JsonProperty("lob")]
        public string LOB { get; set; }
    }
}
