using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class MissedTransaction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("id")]
        [JsonProperty("id")]
        public string? MissingTransactionId { get; set; } = null;

        [BsonElement("transactionId")]
        [JsonProperty("transactionId")]
        public string? TransactionId { get; set; } = null;

        [BsonElement("transactionReferenceNumber")]
        [JsonProperty("transactionReferenceNumber")]
        public string? TransactionReferenceNumber { get; set; } = null;

        [BsonElement("mobileNumber")]
        [JsonProperty("mobileNumber")]
        public string? MobileNumber { get; set; } = null;

        [BsonIgnoreIfNull]
        [BsonElement("isQueued")]
        [JsonProperty("isQueued")]
        public bool IsQueued { get; set; } = false;

        [BsonIgnoreIfNull]
        [BsonElement("queuedDateTime")]
        [JsonProperty("queuedDateTime")]
        public DateTime? QueuedDateTime { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("isReceivedOnQualificationService")]
        [JsonProperty("isReceivedOnQualificationService")]
        public bool IsReceivedOnQualificationService { get; set; } = false;

        [BsonIgnoreIfNull]
        [BsonElement("receivedOnQualificationServiceDateTime")]
        [JsonProperty("receivedOnQualificationServiceDateTime")]
        public DateTime? ReceivedOnQualificationServiceDateTime { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("isReceivedOnExecutionService")]
        [JsonProperty("isReceivedOnExecutionService")]
        public bool IsReceivedOnExecutionService { get; set; } = false;

        [BsonIgnoreIfNull]
        [BsonElement("receivedOnExecutionServiceDateTime")]
        [JsonProperty("receivedOnExecutionServiceDateTime")]
        public DateTime? ReceivedOnExecutionServiceDateTime { get; set; } = null;

        [BsonIgnoreIfNull]
        [BsonElement("isPickedFromTolarance")]
        [JsonProperty("isPickedFromTolarance")]
        public bool IsPickedFromFaultTolarance { get; set; } = false;

        [BsonElement("processedTransaction")]
        [JsonProperty("processedTransaction")]
        public TransactionModel.ProcessedTransaction? ProcessedTransaction { get; set; } = null;

        [BsonElement("transactionDateTime")]
        [JsonProperty("transactionDateTime")]
        public DateTime TransactionDateTime { get; set; }

    }
}
