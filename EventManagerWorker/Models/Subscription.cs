using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Domain.Models.SubscriptionModel
{
    public class Document<T>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("id")]
        [JsonProperty("id")]
        public T Id { get; set; }

        [BsonElement("groupId")]
        [JsonProperty("groupId")]
        public string GroupId { get; set; }

        [BsonElement("createdBy")]
        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("createdAt")]
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedBy")]
        [JsonProperty("updatedBy")]
        public string UpdatedBy { get; set; }

        [BsonElement("updatedAt")]
        [JsonProperty("updatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? UpdatedAt { get; set; }

        [BsonElement("versionId")]
        [JsonProperty("versionId")]
        public string VersionId { get; set; }

        [BsonElement("approver")]
        [JsonProperty("approver")]
        public string Approver { get; set; }

        [BsonElement("approvedAt")]
        [JsonProperty("approvedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ApprovedAt { get; set; }

        [BsonElement("status")]
        [JsonProperty("status")]
        public string Status { get; set; } // Active, 


        [BsonElement("reasonTitle")]
        [JsonProperty("reasonTitle")]
        public string ReasonTitle { get; set; }


        [BsonElement("reasonDescription")]
        [JsonProperty("reasonDescription")]
        public string ReasonDescription { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Bundle
    {
        [BsonIgnoreIfNull]
        [BsonElement("id")]
        [JsonProperty("id")]
        public string Id { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("versionId")]
        [JsonProperty("versionId")]
        public string VersionId { get; set; }

        [BsonElement("bundleGroupId")]
        [JsonProperty("bundleGroupId")]
        public string BundleGroupId { get; set; }

        [BsonElement("name")]
        [JsonProperty("name")]
        public string Name { get; set; }

        [BsonElement("bundlePerceivedValue")]
        [JsonProperty("bundlePerceivedValue")]
        public double BundlePerceivedValue { get; set; }

        [BsonElement("allocatedPerceivedValue")]
        [JsonProperty("allocatedPerceivedValue")]
        public double AllocatedPerceivedValue { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class ContentTab
    {
        [BsonIgnoreIfNull]
        [BsonElement("subBenefits")]
        [JsonProperty("subBenefits")]
        public string SubBenefits { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("subDetails")]
        [JsonProperty("subDetails")]
        public string SubDetails { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class AlertTab
    {
        [BsonIgnoreIfNull]
        [BsonElement("alertTemplate")]
        [JsonProperty("alertTemplate")]
        public string AlertTemplate { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("templateId")]
        [JsonProperty("templateId")]
        public string TemplateId { get; set; } = string.Empty;

    }

    [BsonIgnoreExtraElements]
    public class Subscription : Document<string>
    {
        [BsonIgnoreIfNull]
        [BsonElement("name")]
        [JsonProperty("name")]
        public string Name { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("type")]
        [JsonProperty("type")]
        public string Type { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("startDateTime")]
        [JsonProperty("startDateTime")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime StartDateTime { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("endDateTime")]
        [JsonProperty("endDateTime")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime EndDateTime { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("period")]
        [JsonProperty("period")]
        public Int16 Period { get; set; }

        [BsonElement("companyCode")]
        [JsonProperty("companyCode")]
        public string CompanyCode { get; set; } = string.Empty;

        [BsonIgnoreIfNull]
        [BsonElement("lob")]
        [JsonProperty("lob")]
        public String Lob { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("fee")]
        [JsonProperty("fee")]
        public Double Fee { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("channel")]
        [JsonProperty("channel")]
        public List<string> Channel { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("customerType")]
        [JsonProperty("customerType")]
        public List<string> CustomerType { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("bundle")]
        [JsonProperty("bundle")]
        public Bundle Bundle { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("contentTab")]
        [JsonProperty("contentTab")]
        public ContentTab ContentTab { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("alertTab")]
        [JsonProperty("alertTab")]
        public AlertTab AlertTab { get; set; }
    }
}
