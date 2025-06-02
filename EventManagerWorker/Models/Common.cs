using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Domain.Models.Common
{
    public class VPA
    {
        [BsonElement("id")]
        [JsonProperty("id")]
        public string Id { get; set; }

        [BsonIgnoreIfNull]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("createdDateTime")]
        [JsonProperty("createdDateTime")]
        public DateTime CreatedDatetime { get; set; } = DateTime.Now;


        [BsonElement("status")]
        [JsonProperty("status")]
        public string Status { get; set; }
    }
    public class ReferralObject
    {
        [BsonElement("isReferAndEarn")]
        [JsonProperty("isReferAndEarn")]
        public bool IsReferAndEarn { get; set; }

        [BsonElement("Referrer")]
        [JsonProperty("Referrer")]
        //[RegularExpression(@"^[a-zA-Z0-9 _-]+$", ErrorMessage = "please enter a proper Customer request mobile number")]
        public string Referrer { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class CustomerCommonProperty
    {
        [BsonElement("kycUpgradeFlg")]
        [JsonProperty("kycUpgradeFlg")]
        public string KYCUpgradeFlg { get; set; }

        [BsonElement("destinationMobile")]
        [JsonProperty("destinationMobile")]
        public string DestinationMobile { get; set; }

        [BsonElement("destinationVPAId")]
        [JsonProperty("destinationVPAId")]
        public string DestinationVPAId { get; set; }

        [BsonElement("customerId")]
        [JsonProperty("customerId")]
        public string CustomerId { get; set; }

        [BsonElement("customerType")]
        [JsonProperty("customerType")]
        //[RegularExpression(@"^[a-zA-Z0-9 _-]+$", ErrorMessage = "please enter a proper Customer id")]
        public string CustomerType { get; set; }
    }
    public class Wallet
    {
        [BsonElement("id")]
        [JsonProperty("id")]
        public string Id { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("createdDateTime")]
        [JsonProperty("createdDateTime")]
        public DateTime CreatedDateTime { get; set; } = DateTime.Now;

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("loadDateTime")]
        [JsonProperty("loadDateTime")]
        public DateTime LoadDateTime { get; set; } = DateTime.Now;

        [BsonElement("balance")]
        [JsonProperty("balance")]
        public Double Balance { get; set; }
    }
    namespace CustomerModel
    {
        #region Customer
        public class Flag
        {
            [BsonElement("wallet")]
            [JsonProperty("wallet")]
            public bool Wallet { get; set; }

            [BsonElement("dormant")]
            [JsonProperty("dormant")]
            public bool Dormant { get; set; }

            [BsonIgnoreIfNull]
            [BsonElement("lobFraud")]
            [JsonProperty("lobFraud")]
            public List<string> LobFraud { get; set; } = new List<string>();

            [BsonElement("loyaltyFraud")]
            [JsonProperty("loyaltyFraud")]
            public int LoyaltyFraud { get; set; }

            [BsonElement("isWhitelisted")]
            [JsonProperty("isWhitelisted")]
            public bool? IsWhitelisted { get; set; }

            [BsonElement("globalDeliquient")]
            [JsonProperty("globalDeliquient")]
            public bool GlobalDeliquient { get; set; }
        }
        public class GlobalDeliquient
        {
            [BsonElement("id")]
            [JsonProperty("id")]
            public string Id { get; set; }
        }
        public class KYC
        {
            [BsonElement("status")]
            [JsonProperty("status")]
            public int Status { get; set; }

            [BsonElement("completionTag")]
            [JsonProperty("completionTag")]
            public string CompletionTag { get; set; } = String.Empty;

            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            [BsonElement("completedDateTime")]
            [JsonProperty("completedDateTime")]
            public DateTime CompletedDateTime { get; set; } = DateTime.Now;

            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            [BsonElement("prevDate")]
            [JsonProperty("prevDate")]
            public DateTime PrevDate { get; set; } = DateTime.Now;

        }
        public class Channel
        {
            [BsonElement("source")]
            [JsonProperty("source")]
            public string Source { get; set; }

            [BsonElement("medium")]
            [JsonProperty("medium")]
            public string Medium { get; set; }
        }
        public class Install
        {
            [BsonElement("source")]
            [JsonProperty("source")]
            public string Source { get; set; }

            [BsonIgnoreIfNull]
            [BsonElement("sourceId")]
            [JsonProperty("sourceId")]
            public int? SourceId { get; set; }

            [BsonElement("channel")]
            [JsonProperty("channel")]
            public string Channel { get; set; }

            [BsonIgnoreIfNull]
            [BsonElement("channelId")]
            [JsonProperty("channelId")]
            public int? ChannelId { get; set; }
        }
        public class Segment
        {
            [BsonElement("code")]
            [JsonProperty("code")]
            public string Code { get; set; }

            [BsonElement("isActive")]
            [JsonProperty("isActive")]
            public bool IsActive { get; set; }
        }
        public enum KYCStatus
        {
            [Display(Name = "None")]
            None,

            [Display(Name = "Minimum")]
            Min,

            [Display(Name = "Full")]
            Full
        }
        #endregion
    }
    namespace TransactionModel
    {
        #region Transaction
        public class Wallet
        {
            [BsonElement("id")]
            [JsonProperty("id")]
            public string Id { get; set; }

            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            [BsonIgnoreIfNull]
            [BsonElement("createdDateTime")]
            [JsonProperty("createdDateTime")]
            public DateTime? CreatedDateTime { get; set; } = DateTime.Now;

            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            [BsonIgnoreIfNull]
            [BsonElement("loadDateTime")]
            [JsonProperty("loadDateTime")]
            public DateTime? LoadDateTime { get; set; } = DateTime.Now;
        }
        public class Campaign
        {
            //[BsonRepresentation(BsonType.ObjectId)]
            [BsonElement("id")]
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [BsonElement("rewardedFlg")]
            [JsonProperty("rewardedFlg")]
            public bool RewardedFlg { get; set; }
        }
        public class QR
        {
            [BsonElement("version")]
            [JsonProperty("version")]
            public string Version { get; set; }

            [BsonElement("scanFlg")]
            [JsonProperty("scanFlg")]
            public bool ScanFlg { get; set; }
        }

        public class EMI
        {
            [BsonElement("bounceFlg")]
            [JsonProperty("bounceFlg")]
            public bool BounceFlg { get; set; }

            [BsonElement("amount")]
            [JsonProperty("amount")]
            public double Amount { get; set; }
        }

        public class CustomerDetail
        {
            [BsonElement("loyaltyId")]
            [JsonProperty("loyaltyId")]
            //[BsonRepresentation(BsonType.ObjectId)]
            public string LoyaltyId { get; set; }

            [BsonElement("customerVersionId")]
            [JsonProperty("customerVersionId")]
            //[BsonRepresentation(BsonType.ObjectId)]
            public string CustomerVersionId { get; set; }

        }

        [BsonIgnoreExtraElements]
        public class Customer : CustomerCommonProperty
        {
            [BsonElement("mobileNumber")]
            [JsonProperty("mobileNumber")]
            public string MobileNumber { get; set; }

            [BsonElement("vpa")]
            [JsonProperty("vpa")]
            public Common.VPA VPA { get; set; }

            [BsonElement("uniqueCustomerCode")]
            [JsonProperty("uniqueCustomerCode")]
            public string UniqueCustomerCode { get; set; }
        }

        public class Voucher
        {
            [BsonElement("code")]
            [JsonProperty("code")]
            public string Code { get; set; }

            [BsonElement("type")]
            [JsonProperty("type")]
            public string Type { get; set; }

            [BsonElement("denomination")]
            [JsonProperty("denomination")]
            public string Denomination { get; set; }
        }

        public class Payment
        {
            [BsonElement("paymentInstrument")]
            [JsonProperty("paymentInstrument")]
            public string PaymentInstrument { get; set; }

            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            [BsonElement("paymentDate")]
            [JsonProperty("paymentDate")]
            public DateTime? PaymentDate { get; set; } = DateTime.Now;

            [BsonElement("paymentMode")]
            [JsonProperty("paymentMode")]
            public string PaymentMode { get; set; }

            [BsonElement("amount")]
            [JsonProperty("amount")]
            public double Amount { get; set; }
        }

        public class UTM
        {
            [BsonElement("source")]
            [JsonProperty("source")]
            public string Source { get; set; }

            [BsonElement("campaign")]
            [JsonProperty("campaign")]
            public string Campaign { get; set; }

            [BsonElement("medium")]
            [JsonProperty("medium")]
            public string Medium { get; set; }
        }

        public class Biller
        {
            [BsonElement("id")]
            [JsonProperty("id")]
            public string Id { get; set; }

            [BsonElement("category")]
            [JsonProperty("category")]
            public string Category { get; set; }
        }

        public class MerchantOrDealer
        {
            [BsonElement("groupId")]
            [JsonProperty("groupId")]
            public string GroupId { get; set; }

            [BsonElement("id")]
            [JsonProperty("id")]
            public string Id { get; set; }

            [BsonElement("category")]
            [JsonProperty("category")]
            public string Category { get; set; }

            [BsonElement("source")]
            [JsonProperty("source")]
            public string Source { get; set; }
        }

        [BsonIgnoreExtraElements]
        public class TransactionDetail
        {
            [BsonElement("refNumber")]
            [JsonProperty("refNumber")]
            public string RefNumber { get; set; }

            [BsonElement("isRedeem")]
            [JsonProperty("isRedeem")]
            public bool IsRedeem { get; set; }

            [BsonElement("type")]
            [JsonProperty("type")]
            public string Type { get; set; } // Holds Payment

            [BsonElement("status")]
            [JsonProperty("status")]
            public string Status { get; set; }

            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            [BsonElement("dateTime")]
            [JsonProperty("dateTime")]
            public DateTime DateTime { get; set; } = DateTime.Now;

            [BsonElement("amount")]
            [JsonProperty("amount")]
            public double Amount { get; set; }

            [BsonElement("emandate")]
            [JsonProperty("emandate")]
            public bool Emandate { get; set; } = false;

            [BsonElement("biller")]
            [JsonProperty("biller")]
            public Common.TransactionModel.Biller Biller { get; set; }

            [BsonElement("loanAmount")]
            [JsonProperty("loanAmount")]
            public double? LoanAmount { get; set; }

            [BsonElement("merchantOrDealer")]
            [JsonProperty("merchantOrDealer")]
            public Common.TransactionModel.MerchantOrDealer MerchantOrDealer { get; set; }

            [BsonElement("qr")]
            [JsonProperty("qr")]
            public Common.TransactionModel.QR QR { get; set; }

            [BsonElement("emi")]
            [JsonProperty("emi")]
            public Common.TransactionModel.EMI EMI { get; set; }

            [BsonElement("wallet")]
            [JsonProperty("wallet")]
            public Common.TransactionModel.Wallet Wallet { get; set; }

            [BsonElement("customer")]
            [JsonProperty("customer")]
            public Common.TransactionModel.Customer Customer { get; set; }

            [BsonElement("voucher")]
            [JsonProperty("voucher")]
            public Common.TransactionModel.Voucher Voucher { get; set; }

            [BsonElement("payments")]
            [JsonProperty("payments")]
            public List<Common.TransactionModel.Payment> Payments { get; set; }

            [BsonElement("referAndEarn")]
            [JsonProperty("referAndEarn")]
            public ReferralObject ReferAndEarn { get; set; }

            [BsonElement("subscriptionId")]
            [JsonProperty("subscriptionId")]
            public string SubscriptionId { get; set; }

            [BsonIgnoreIfNull]
            [BsonElement("isTnCAccepted")]
            [JsonProperty("isTnCAccepted")]
            public bool IsTnCAccepted { get; set; }
        }
        #endregion
    }


}
