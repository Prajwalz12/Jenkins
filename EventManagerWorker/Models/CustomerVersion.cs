using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Domain.Models.CustomerModel
{
    public class CustomerVersion
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("hId")]
        [JsonProperty("hId")]
        public string CustomerVersionId { get; set; }

        [BsonElement("mobileNumber")]
        [JsonProperty("mobileNumber")]
        public string MobileNumber { get; set; }

        [BsonElement("type")]
        [JsonProperty("type")]
        public string Type { get; set; }

        [BsonElement("typeId")]
        [JsonProperty("typeId")]
        public int TypeId { get; set; }

        ////[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("signUpDate")]
        [JsonProperty("signUpDate")]
        public DateTime SignUpDate { get; set; }

        [BsonElement("wallet")]
        [JsonProperty("wallet")]
        public Common.Wallet Wallet { get; set; }

        [BsonElement("walletBalance")]
        [JsonProperty("walletBalance")]
        public double WalletBalance { get; set; }

        [BsonElement("vpa")]
        [JsonProperty("vpa")]
        public Common.VPA VPA { get; set; }

        [BsonElement("kyc")]
        [JsonProperty("kyc")]
        public Common.CustomerModel.KYC KYC { get; set; }

        [BsonElement("flags")]
        [JsonProperty("flags")]
        public Common.CustomerModel.Flag Flags { get; set; }

        [BsonElement("install")]
        [JsonProperty("install")]
        public Common.CustomerModel.Install Install { get; set; }

        [BsonElement("segments")]
        [JsonProperty("segments")]
        public List<Common.CustomerModel.Segment> Segments { get; set; }

        ////[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("createdAt")]
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        ////[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("updatedAt")]
        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [BsonElement("uniqueCustomerCode")]
        [JsonProperty("uniqueCustomerCode")]
        public string UniqueCustomerCode { get; set; }

        #region Subscription Section
        [BsonElement("subscriptionType")]
        [JsonProperty("subscriptionType")]
        public string SubscriptionType { get; set; }

        [BsonElement("subscriptionId")]
        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }

        [BsonElement("subscriptionName")]
        [JsonProperty("subscriptionName")]
        public string SubscriptionName { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("subscriptionActivationDate")]
        [JsonProperty("subscriptionActivationDate")]
        public DateTime SubscriptionActivationDate { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("subscriptionEndDate")]
        [JsonProperty("subscriptionEndDate")]
        public DateTime SubscriptionEndDate { get; set; }

        #endregion

        #region New Optional Field
        [BsonElement("firstName")]
        [JsonProperty("firstName")]
        public string? FirstName { get; set; } = String.Empty;

        [BsonElement("lastName")]
        [JsonProperty("lastName")]
        public string? LastName { get; set; } = String.Empty;

        [BsonElement("name")]
        [JsonProperty("name")]
        public string? Name { get; set; } = String.Empty;

        [BsonElement("email")]
        [JsonProperty("email")]
        public string? Email { get; set; } = String.Empty;
        #endregion
    }
}
