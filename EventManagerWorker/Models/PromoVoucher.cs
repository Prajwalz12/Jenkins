using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Domain.Models.RewardModel
{

    #region PromoVoucher Section

    [BsonIgnoreExtraElements]
    public class PromoRewardConfiguration
    {
        [BsonElement("voucherType")]
        [JsonProperty("voucherType")]
        public int VoucherType { get; set; } = 0;   //1 : Cashback, 2 : Inline discount

        [BsonElement("skuId")]
        [JsonProperty("skuId")]
        public int SkuId { get; set; } = 0;

        [BsonElement("valueType")]
        [JsonProperty("valueType")]
        public int ValueType { get; set; } = 0;     //1: Fixed, 2 : Percentage

        [BsonElement("fixValue")]
        [JsonProperty("fixValue")]
        public RewardValueTypeFixed FixValue { get; set; }

        [BsonElement("percentageValue")]
        [JsonProperty("percentageValue")]
        public RewardValueTypePercentage PercentageValue { get; set; }

        [BsonElement("validity")]
        [JsonProperty("validity")]
        public int Validity { get; set; } = 0;    //1: Dynamic, 2:Static

        [BsonElement("staticValidDate")]
        [JsonProperty("staticValidDate")]
        public string StaticValidDate { get; set; }

        [BsonElement("dynamicValidDay")]
        [JsonProperty("dynamicValidDay")]
        public int DynamicValidDay { get; set; } = 0;

        [BsonElement("quantity")]
        [JsonProperty("quantity")]
        public int Quantity { get; set; } = 0;


        [BsonIgnoreIfNull]
        [BsonElement("minimumPurchaseValue")]
        [JsonProperty("minimumPurchaseValue")]
        public decimal? MinimumPurchaseValue { get; set; } = 0;
    }

    [BsonIgnoreExtraElements]
    public class RewardValueTypeFixed
    {
        [BsonElement("value")]
        [JsonProperty("value")]
        public decimal Value { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class RewardValueTypePercentage
    {
        [BsonElement("value")]
        [JsonProperty("value")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Value { get; set; }
        [BsonElement("maximumCashback")]
        [JsonProperty("maximumCashback")]
        public decimal MaximumCashback { get; set; }
    }
    #endregion

    [BsonIgnoreExtraElements]
    public class PromoVoucher
    {

        [BsonElement("promoRewardConfiguration")]
        [JsonProperty("promoRewardConfiguration")]
        public PromoRewardConfiguration PromoRewardConfiguration { get; set; }

        [BsonElement("promoVoucherIssued")]
        [JsonProperty("promoVoucherIssued")]
        public List<PromoVoucherIssued> PromoVoucherIssued { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class PromoVoucherIssued
    {
        [BsonElement("mobileNumber")]
        [JsonProperty("mobileNumber")]
        public string MobileNumber { get; set; }


        [BsonElement("campaignId")]
        [JsonProperty("campaignId")]
        public string CampaignId { get; set; }
        [BsonElement("voucherCode")]
        [JsonProperty("voucherCode")]
        public string VoucherCode { get; set; }
        [BsonElement("pin")]
        [JsonProperty("pin")]
        public string Pin { get; set; }
        [BsonElement("expireDate")]
        [JsonProperty("expireDate")]
        public DateTime? ExpireDate { get; set; }
        [BsonElement("expireDay")]
        [JsonProperty("expireDay")]
        public int ExpireDay { get; set; }
        [BsonElement("minimumPurchaseValue")]
        [JsonProperty("minimumPurchaseValue")]
        public decimal? MinimumPurchaseValue { get; set; } = 0;

        [BsonElement("uniqueCustomerCode")]
        [JsonProperty("uniqueCustomerCode")]
        public string UniqueCustomerCode { get; set; }
    }
}

