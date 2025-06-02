using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Domain.Models.CustomerModel
{
    public class CustomerEvent
    {
        //[BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("id")]
        [JsonProperty("id")]
        public string Id { get; set; }

        [BsonElement("campaignId")]
        [JsonProperty("campaignId")]
        public List<string> CampaignId { get; set; } = null;

        //[BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("loyaltyId")]
        [JsonProperty("loyaltyId")]
        public string LoyaltyId { get; set; }

        [BsonIgnore]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("customerVersionId")]
        [JsonIgnore]
        [JsonProperty("customerVersionId")]
        public string CustomerVersionId { get; set; }

        [BsonElement("mobileNumber")]
        [JsonProperty("mobileNumber")]
        public string MobileNumber { get; set; }

        [BsonElement("eventCode")]
        [JsonProperty("eventCode")]
        public string EventCode { get; set; }

        [BsonElement("childEventCode")]
        [JsonProperty("childEventCode")]
        public string ChildEventCode { get; set; } = null;

        [BsonElement("eventType")]
        [JsonProperty("eventType")]
        public string EventType { get; set; }

        [BsonElement("paymentInstrument")]
        [JsonProperty("paymentInstrument")]
        public List<string> PaymentInstrument { get; set; }

        [BsonElement("paymentCategory")]
        [JsonProperty("paymentCategory")]
        public string PaymentCategory { get; set; }

        [BsonElement("amount")]
        [JsonProperty("amount")]
        public double Amount { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("createdAt")]
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("updatedAt")]
        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("txnDateTime")]
        [JsonProperty("txnDateTime")]
        public DateTime TxnDateTime { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("transactionId")]
        [JsonProperty("transactionId")]
        public string? TransactionId { get; set; } = null;

        [BsonIgnoreIfNull]
        [BsonElement("transactionReferenceNumber")]
        [JsonProperty("transactionReferenceNumber")]
        public string? TransactionReferenceNumber { get; set; } = null;

        [BsonElement("uniqueCustomerCode")]
        [JsonProperty("uniqueCustomerCode")]
        public string UniqueCustomerCode { get; set; }
    }
}
