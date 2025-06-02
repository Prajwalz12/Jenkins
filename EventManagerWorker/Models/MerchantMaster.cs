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
    public class MerchantMaster
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("id")]
        [JsonProperty("id")]
        public string Id { get; set; }
        [BsonElement("status")]
        [JsonProperty("status")]
        public int Status { get; set; }
        [BsonElement("createdDate")]
        [JsonProperty("createdDate")]
        public DateTime? Createddate { get; set; }
        [BsonElement("createdBy")]
        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }
        [BsonElement("updatedDate")]
        [JsonProperty("updatedDate")]
        public DateTime? Updateddate { get; set; }
        [BsonElement("updatedBy")]
        [JsonProperty("updatedBy")]
        public string UpdatedBy { get; set; }
        public int MerchantType { get; set; }
        [BsonElement("lob")]
        [JsonProperty("lob")]
        public string LOB { get; set; }
        [BsonElement("profitCentreCode")]
        [JsonProperty("profitCentreCode")]
        public string ProfitCentreCode { get; set; }
        [BsonElement("costCentre")]
        [JsonProperty("costCentre")]
        public string CostCentre { get; set; }
        public string GroupMerchantCode { get; set; }
        [BsonElement("finnoneDealerCode")]
        [JsonProperty("finnoneDealerCode")]
        public string FinnoneDealerCode { get; set; }
        [BsonElement("merchantID")]
        [JsonProperty("merchantID")]
        public string MerchantID { get; set; }
        [BsonElement("merchantName")]
        [JsonProperty("merchantName")]
        public string MerchantName { get; set; }
        public string MerchantCityCode { get; set; }
        [BsonElement("merchantOnboardingDate")]
        [JsonProperty("merchantOnboardingDate")]
        public DateTime? MerchantOnboardingDate { get; set; }
        [BsonElement("tripleReward")]
        [JsonProperty("tripleReward")]
        public int TripleReward { get; set; }
        [BsonElement("fileId")]
        [JsonProperty("fileId")]
        public int FileID { get; set; }
        [BsonElement("version")]
        [JsonProperty("version")]
        public string Version { get; set; }
        [BsonElement("merchantCategory")]
        [JsonProperty("merchantCategory")]
        public string MerchantCategory { get; set; }

        [BsonElement("iPAddress")]
        [JsonProperty("iPAddress")]
        public string IPAddress { get; set; }

        [BsonElement("subLob")]
        [JsonProperty("subLob")]
        public string SubLOB { get; set; }

        [BsonElement("categoryCode")]
        [JsonProperty("categoryCode")]
        public string CategoryCode { get; set; }
        [BsonElement("source")]
        [JsonProperty("source")]
        public string source { get; set; }
        [BsonElement("isNonMerchant")]
        [JsonProperty("isNonMerchant")]
        public int isNonMerchant { get; set; }
        [BsonElement("mType")]
        [JsonProperty("mType")]
        public string MType { get; set; }
        [BsonElement("groupFinnoneCode")]
        [JsonProperty("groupFinnoneCode")]
        public string GroupFinnoneCode { get; set; }

        [BsonElement("groupMerchantId")]
        [JsonProperty("groupMerchantId")]
        public string GroupMerchantId { get; set; }

    }

    public class MerchantEnumRequest
    {
        public string Category { get; set; }
        public string MerchantId { get; set; }
        public string GroupMerchantId { get; set; }
        public int? TripleReward { get; set; }
        public string Source { get; set; }
        public string MerchantType { get; set; }
    }
}
