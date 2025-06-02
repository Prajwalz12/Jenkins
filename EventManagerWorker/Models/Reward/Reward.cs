using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Domain.Models.RewardModel
{
    [BsonIgnoreExtraElements]
    public class CommonRewardCollection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("id")]
        [BsonElement("id")]
        public string Id { get; set; }

        [BsonElement("type")]
        [JsonProperty("type")]
        public string RewardType { get; set; }

        [BsonElement("mobileNumber")]
        [JsonProperty("mobileNumber")]
        public string MobileNumber { get; set; }

        [BsonElement("uniqueCustomerCode")]
        [JsonProperty("uniqueCustomerCode")]
        public string UniqueCustomerCode { get; set; } = string.Empty;

        //"None|Suspect"
        [BsonElement("custLoyaltyFraudStatus")]
        [JsonProperty("custLoyaltyFraudStatus")]
        public int CustLoyaltyFraudStatus { get; set; }

        [BsonElement("txnId")]
        [JsonProperty("txnId")]
        public string TxnId { get; set; }

        [BsonElement("txnRefId")]
        [JsonProperty("txnRefId")]
        public string TxnRefId { get; set; }

        [BsonElement("txnDate")]
        [JsonProperty("txnDate")]// from BFL
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime TxnDate { get; set; } = DateTime.Now;

        [BsonElement("lob")]
        [JsonProperty("lob")]
        public string LOB { get; set; }

        [BsonElement("campaignId")]
        [JsonProperty("campaignId")]
        public string CampaignId { get; set; }

        [BsonElement("issueInState")]
        [JsonProperty("issueInState")]
        public string IssueInState { get; set; }

        // "Null|DateTime",
        [BsonElement("lockExpireDate")]
        [JsonProperty("lockExpireDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]

        public DateTime? LockExpireDate { get; set; }

        [BsonElement("isLockExpires")]
        [JsonProperty("isLockExpires")]
        public bool IsLockExpires { get; set; }


        [BsonElement("paymentCategory")]
        [JsonProperty("paymentCategory")]
        public string PaymentCategory { get; set; }

        [BsonElement("paymentInstruments")]
        [JsonProperty("paymentInstruments")]
        public List<string> PaymentInstruments { get; set; }


        [BsonElement("merchantId")]
        [JsonProperty("merchantId")]
        public string MerchantId { get; set; }

        [BsonElement("grpMerchantId")]
        [JsonProperty("grpMerchantId")]
        public string GrpMerchantId { get; set; }

        [BsonElement("expiryDate")]
        [JsonProperty("expiryDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]

        public DateTime ExpiryDate { get; set; } = DateTime.Now;

        [BsonElement("issueDate")]
        [JsonProperty("issueDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]

        public DateTime IssueDate { get; set; } = DateTime.Now;

        [BsonElement("narration")]
        [JsonProperty("narration")]
        public string Narration { get; set; }

        [BsonElement("amount")]
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [BsonElement("eventId")]
        [JsonProperty("eventId")]
        public string EventId { get; set; }

        [BsonElement("childEventCode")]
        [JsonProperty("childEventCode")]
        public string ChildEventCode { get; set; } = null; // Dileep : Added ChildEventCode

        [BsonElement("parentTransactionId")]
        [JsonProperty("parentTransactionId")]
        public string ParentTransactionId { get; set; } = null;//JD,Dipeep: Added ParentTransactionId

        [BsonElement("billerName")]
        [JsonProperty("billerName")]
        public string BillerName { get; set; }

        [BsonElement("billerCategory")]
        [JsonProperty("billerCategory")]
        public string BillerCategory { get; set; }

        [BsonElement("merchantName")]
        [JsonProperty("merchantName")]
        public string MerchantName { get; set; }

        [BsonElement("promoCode")]
        [JsonProperty("promoCode")]
        public string PromoCode { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("voucherDetail")]
        [JsonProperty("voucherDetail")]
        public VoucherDetail VoucherDetail { get; set; } = null;

        [BsonIgnoreIfNull]
        [BsonElement("issueVoucherDetail")]
        [JsonProperty("issueVoucherDetail")]
        public IssueVoucherDetail IssueVoucherDetail { get; set; } = null;

        [BsonElement("txnType")]
        [JsonProperty("txnType")]
        public int TxnType { get; set; } = 0;

        [BsonElement("retryAttempt")]
        [JsonProperty("retryAttempt")]
        public int? RetryAttempt { get; set; } = null;

        [BsonElement("issueStatusMessage")]
        [JsonProperty("issueStatusMessage")]
        public string IssueStatusMessage { get; set; } = null;

        [BsonElement("issueStatusCode")]
        [JsonProperty("issueStatusCode")]
        public int? IssueStatusCode { get; set; } = null;

        [BsonElement("ctaUrl")]
        [JsonProperty("ctaUrl")]
        public string CTAUrl { get; set; } = null; //added :chetan

        [BsonElement("unlockTermAndCondition")]
        [JsonProperty("unlockTermAndCondition")]
        public string UnlockTermAndCondition { get; set; } = null;

        [BsonElement("channelCode")]
        [JsonProperty("channelCode")]
        public string ChannelCode { get; set; }

        //0=normal,1=referee,2=referral
        [BsonElement("isReferEarn")]
        [JsonProperty("isReferEarn")]
        public int? IsReferEarn { get; set; } = 0;
    }
    [BsonIgnoreExtraElements]
    public class VoucherDetail
    {
        [BsonElement("txnType")]
        [JsonProperty("txnType")]
        public int TxnType { get; set; }

        [BsonElement("catalog")]
        [JsonProperty("catalog")]
        public string Catalog { get; set; }

        [BsonElement("category")]
        [JsonProperty("category")]
        public string Category { get; set; }

        [BsonElement("brand")]
        [JsonProperty("brand")]
        public string Brand { get; set; }

        [BsonElement("brandCode")]
        [JsonProperty("brandCode")]
        public string BrandCode { get; set; }

        [BsonElement("denomination")]
        [JsonProperty("denomination")]
        public int Denomination { get; set; }

        [BsonElement("quantity")]
        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [BsonElement("voucherCost")]
        [JsonProperty("voucherCost")]
        public decimal VoucherCost { get; set; }

        [BsonElement("voucherCodeSource")]
        [JsonProperty("voucherCodeSource")]
        public string VoucherCodeSource { get; set; }

        [BsonElement("channelCode")]
        [JsonProperty("channelCode")]
        public string ChannelCode { get; set; }

        [BsonElement("sku")]
        [JsonProperty("sku")]
        public string SKU { get; set; }

        [BsonElement("bundleId")]
        [JsonProperty("bundleId")]
        public string BundleId { get; set; }

        [BsonElement("marchantOTT")]
        [JsonProperty("marchantOTT")]
        public string MarchantOTT { get; set; } // OTT / Merchant
    }
    [BsonIgnoreExtraElements]
    public class IssueVoucherDetail
    {
        [BsonElement("sku")]
        [JsonProperty("sku")]
        public string SKU { get; set; }

        [BsonElement("transactionId")]
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [BsonElement("statusCode")]
        [JsonProperty("statusCode")]
        public string StatusCode { get; set; }

        [BsonElement("statusMessage")]
        [JsonProperty("statusMessage")]
        public string StatusMessage { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class Reward : CommonRewardCollection
    {
        [BsonElement("isConvertedFromCashback")]
        [JsonProperty("isConvertedFromCashback")]
        public bool IsConvertedFromCashback { get; set; }

        [BsonElement("cashback")]
        [JsonProperty("cashback")]
        public double Cashback { get; set; }

        [BsonElement("points")]
        [JsonProperty("points")]
        public double Points { get; set; }

        //[BsonElement("txnType")]
        //[JsonProperty("txnType")]
        //public int TxnType { get; set; }

        [BsonElement("isAccure")]
        [JsonProperty("isAccure")]
        public bool IsAccure { get; set; }

        [BsonElement("isScratched")]
        [JsonProperty("isScratched")]
        public bool IsScratched { get; set; }

        [BsonElement("activeDate")]
        [JsonProperty("activeDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]

        public DateTime? ActiveDate { get; set; }
        [BsonElement("activationDate")]
        [JsonProperty("activationDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]

        public DateTime? ActivationDate { get; set; }

        [BsonElement("promoVoucher")]
        [JsonProperty("promoVoucher")]
        public PromoVoucher PromoVoucher { get; set; }

    }
    [BsonIgnoreExtraElements]
    public class TempReward : CommonRewardCollection
    {

        [BsonElement("brandId")]
        [JsonProperty("brandId")]
        public int BrandId { get; set; }
        [BsonElement("activeDate")]
        [JsonProperty("activeDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]

        public DateTime? ActiveDate { get; set; }
        [BsonElement("activationDate")]
        [JsonProperty("activationDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]

        public DateTime? ActivationDate { get; set; }

        [BsonElement("isConvertedFromCashback")]
        [JsonProperty("isConvertedFromCashback")]
        public bool IsConvertedFromCashback { get; set; }

        [BsonElement("cashback")]
        [JsonProperty("cashback")]
        public double Cashback { get; set; }

        [BsonElement("points")]
        [JsonProperty("points")]
        public double Points { get; set; }

        //[BsonElement("txnType")]
        //[JsonProperty("txnType")]
        //public int TxnType { get; set; }

        [BsonElement("isAccure")]
        [JsonProperty("isAccure")]
        public bool IsAccure { get; set; }

        [BsonElement("isScratched")]
        [JsonProperty("isScratched")]
        public bool IsScratched { get; set; }

        [BsonElement("rewardLogId")]
        [JsonProperty("rewardLogId")]
        public string RewardLogId { get; set; }

        //[BsonElement("retryAttempt")]
        //[JsonProperty("retryAttempt")]
        //public int? RetryAttempt { get; set; } = null;

        //[BsonElement("issueStatusMessage")]
        //[JsonProperty("issueStatusMessage")]
        //public string IssueStatusMessage { get; set; } = null;

        //[BsonElement("issueStatusCode")]
        //[JsonProperty("issueStatusCode")]
        //public int? IssueStatusCode { get; set; } = null;

        [BsonElement("promoVoucher")]
        [JsonProperty("promoVoucher")]
        public PromoVoucher PromoVoucher { get; set; }

        [BsonElement("lockStatus")]
        [JsonProperty("lockStatus")]
        public bool? LockStatus { get; set; } = false;


    }
    [BsonIgnoreExtraElements]
    public class TempRewardHistory : CommonRewardCollection
    {

        [BsonElement("brandId")]
        [JsonProperty("brandId")]
        public int BrandId { get; set; }
        [BsonElement("activeDate")]
        [JsonProperty("activeDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]

        public DateTime? ActiveDate { get; set; }
        [BsonElement("activationDate")]
        [JsonProperty("activationDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]

        public DateTime? ActivationDate { get; set; }

        [BsonElement("isConvertedFromCashback")]
        [JsonProperty("isConvertedFromCashback")]
        public bool IsConvertedFromCashback { get; set; }

        [BsonElement("cashback")]
        [JsonProperty("cashback")]
        public double Cashback { get; set; }

        [BsonElement("points")]
        [JsonProperty("points")]
        public double Points { get; set; }

        //[BsonElement("txnType")]
        //[JsonProperty("txnType")]
        //public int TxnType { get; set; }

        [BsonElement("isAccure")]
        [JsonProperty("isAccure")]
        public bool IsAccure { get; set; }

        [BsonElement("isScratched")]
        [JsonProperty("isScratched")]
        public bool IsScratched { get; set; }

        [BsonElement("rewardLogId")]
        [JsonProperty("rewardLogId")]
        public string RewardLogId { get; set; }

        //[BsonElement("retryAttempt")]
        //[JsonProperty("retryAttempt")]
        //public int? RetryAttempt { get; set; } = null;

        //[BsonElement("issueStatusMessage")]
        //[JsonProperty("issueStatusMessage")]
        //public string IssueStatusMessage { get; set; } = null;

        //[BsonElement("issueStatusCode")]
        //[JsonProperty("issueStatusCode")]
        //public int? IssueStatusCode { get; set; } = null;

        [BsonElement("promoVoucher")]
        [JsonProperty("promoVoucher")]
        public PromoVoucher PromoVoucher { get; set; }

        [BsonElement("lockStatus")]
        [JsonProperty("lockStatus")]
        public bool? LockStatus { get; set; } = false;


    }

    [BsonIgnoreExtraElements]
    public class FraudReward : CommonRewardCollection
    {
        [BsonElement("brandId")]
        [JsonProperty("brandId")]
        public int BrandId { get; set; }

        [BsonElement("activeDate")]
        [JsonProperty("activeDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ActiveDate { get; set; }

        [BsonElement("activationDate")]
        [JsonProperty("activationDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ActivationDate { get; set; }

        [BsonElement("isConvertedFromCashback")]
        [JsonProperty("isConvertedFromCashback")]
        public bool IsConvertedFromCashback { get; set; }

        [BsonElement("cashback")]
        [JsonProperty("cashback")]
        public double Cashback { get; set; }

        [BsonElement("points")]
        [JsonProperty("points")]
        public double Points { get; set; }

        //[BsonElement("txnType")]
        //[JsonProperty("txnType")]
        //public int TxnType { get; set; }

        [BsonElement("isAccure")]
        [JsonProperty("isAccure")]
        public bool IsAccure { get; set; }

        [BsonElement("isScratched")]
        [JsonProperty("isScratched")]
        public bool IsScratched { get; set; }

        [BsonElement("promoVoucher")]
        [JsonProperty("promoVoucher")]
        public PromoVoucher PromoVoucher { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class TransactionReward : CommonRewardCollection
    {
        [BsonElement("isConvertedFromCashback")]
        [JsonProperty("isConvertedFromCashback")]
        public bool IsConvertedFromCashback { get; set; }

        [BsonElement("cashback")]
        [JsonProperty("cashback")]
        public double Cashback { get; set; }

        [BsonElement("points")]
        [JsonProperty("points")]
        public double Points { get; set; }

        //[BsonElement("txnType")]
        //[JsonProperty("txnType")]
        //public int TxnType { get; set; }

        [BsonElement("isAccure")]
        [JsonProperty("isAccure")]
        public bool IsAccure { get; set; }

        [BsonElement("isScratched")]
        [JsonProperty("isScratched")]
        public bool IsScratched { get; set; }

        [BsonElement("activationDate")]
        [JsonProperty("activationDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ActivationDate { get; set; }

        [BsonElement("promoVoucher")]
        [JsonProperty("promoVoucher")]
        public PromoVoucher PromoVoucher { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class PointReward : CommonRewardCollection
    {
        [BsonElement("parentBundleId")]
        [JsonProperty("parentBundleId")]
        public string ParentBundleId { get; set; }

        [BsonElement("points")]
        [JsonProperty("points")]
        public double Points { get; set; }

        //[BsonElement("txnType")]
        //[JsonProperty("txnType")]
        //public int TxnType { get; set; }

        [BsonElement("isConvertedFromCashback")]
        [JsonProperty("isConvertedFromCashback")]
        public bool IsConvertedFromCashback { get; set; }


        [BsonElement("brandId")]
        [JsonProperty("brandId")]
        public int BrandId { get; set; }


        [BsonElement("activeDate")]
        [JsonProperty("activeDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]

        public DateTime ActiveDate { get; set; }

        [BsonElement("activationDate")]
        [JsonProperty("activationDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]

        public DateTime ActivationDate { get; set; } = DateTime.Now;

    }
    [BsonIgnoreExtraElements]
    public class CashbackReward : CommonRewardCollection
    {
        [BsonElement("parentBundleId")]
        [JsonProperty("parentBundleId")]
        public string ParentBundleId { get; set; }

        [BsonElement("cashback")]
        [JsonProperty("cashback")]
        public double Cashback { get; set; }

        [BsonElement("isAccure")]
        [JsonProperty("isAccure")]
        public bool IsAccure { get; set; }

        [BsonElement("isScratched")]
        [JsonProperty("isScratched")]
        public bool IsScratched { get; set; }

    }
    [BsonIgnoreExtraElements]
    public class VoucherReward : CommonRewardCollection
    {
        [BsonElement("parentBundleId")]
        [JsonProperty("parentBundleId")]
        public string ParentBundleId { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class FraudLockedRewards
    {
        [BsonElement("id")]
        [JsonProperty("id")]
        public int Id { get; set; }

        [BsonElement("insertionDate")]
        [JsonProperty("insertionDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]

        public DateTime? InsertionDate { get; set; }

        [BsonElement("modificationDate")]
        [JsonProperty("modificationDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]

        public DateTime? ModificationDate { get; set; }

        [BsonElement("mobileNumber")]
        [JsonProperty("mobileNumber")]
        public string MobileNumber { get; set; }

        [BsonElement("ruleId")]
        [JsonProperty("ruleId")]
        public string RuleId { get; set; }

        [BsonElement("rewardType")]
        [JsonProperty("rewardType")]
        public int RewardType { get; set; }

        [BsonElement("pointTypeID")]
        [JsonProperty("pointTypeID")]
        public int PointTypeID { get; set; }

        [BsonElement("transactionId")]
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [BsonElement("amount")]
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [BsonElement("sourceType")]
        [JsonProperty("sourceType")]
        public int SourceType { get; set; }

        [BsonElement("status")]
        [JsonProperty("status")]
        public int Status { get; set; }

        [BsonElement("date")]
        [JsonProperty("date")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]

        public DateTime? Date { get; set; }

        [BsonElement("remark")]
        [JsonProperty("remark")]
        public string Remark { get; set; }

        [BsonElement("updateAt")]
        [JsonProperty("updateAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]

        public DateTime? UpdateAt { get; set; }

        [BsonElement("updateBy")]
        [JsonProperty("updateBy")]
        public string UpdateBy { get; set; }

        [BsonElement("uniqueCustomerCode")]
        [JsonProperty("uniqueCustomerCode")]
        public string UniqueCustomerCode { get; set; }

    }
}
