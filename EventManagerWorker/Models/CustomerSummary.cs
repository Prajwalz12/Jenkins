using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;

namespace Domain.Models.CustomerModel
{
    public class PointCashback
    {
        [BsonElement("lifeTimeEarn")]
        [JsonProperty("lifeTimeEarn")]
        public double LifeTimeEarn { get; set; }

        [BsonElement("lifeTimeExpired")]
        [JsonProperty("lifeTimeExpired")]
        public double LifeTimeExpired { get; set; }

        [BsonElement("lifeTimeRedeemed")]
        [JsonProperty("lifeTimeRedeemed")]
        public double LifeTimeRedeemed { get; set; }

        [BsonElement("availableBalance")]
        [JsonProperty("availableBalance")]
        public double AvailableBalance { get; set; }

        [BsonElement("currentMonthEarned")]
        [JsonProperty("currentMonthEarned")]
        public double CurrentMonthEarned { get; set; }

        [BsonElement("currentMonthRedeemed")]
        [JsonProperty("currentMonthRedeemed")]
        public double CurrentMonthRedeemed { get; set; }

        [BsonElement("currentMonthExpired")]
        [JsonProperty("currentMonthExpired")]
        public double CurrentMonthExpired { get; set; }

        [BsonElement("expireInNext30Days")]
        [JsonProperty("expireInNext30Days")]
        public double ExpireInNext30Days { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class Point : PointCashback
    {
        [BsonElement("blockedPoints")]
        [JsonProperty("blockedPoints")]
        public double BlockedPoints { get; set; }

        [BsonElement("blockedPromoPoints")]
        [JsonProperty("blockedPromoPoints")]
        public double BlockedPromoPoints { get; set; }

        [BsonElement("promoPoints")]
        [JsonProperty("promoPoints")]
        public double PromoPoints { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class Cashback : PointCashback
    {
        [BsonIgnoreIfNull]
        [BsonElement("cashbackUnClaimed")]
        [JsonProperty("cashbackUnClaimed")]
        public int CashbackUnClaimed { get; set; } = 0;
    }
    [BsonIgnoreExtraElements]
    public class Voucher
    {
        [BsonElement("vouchersEarned")]
        [JsonProperty("vouchersEarned")]
        public int VouchersEarned { get; set; }

        [BsonElement("vouchersPurchased")]
        [JsonProperty("vouchersPurchased")]
        public int VouchersPurchased { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("vouchersUnClaimed")]
        [JsonProperty("vouchersUnClaimed")]
        public int VouchersUnClaimed { get; set; } = 0;

        [BsonIgnoreIfNull]
        [BsonElement("vouchersClaimed")]
        [JsonProperty("vouchersClaimed")]
        public int VouchersClaimed { get; set; } = 0;

        [BsonIgnoreIfNull]
        [BsonElement("vouchersUnClaimedExpired")]
        [JsonProperty("vouchersUnClaimedExpired")]
        public int VouchersUnClaimedExpired { get; set; } = 0;
    }
    public class CustomerSummary
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("id")]
        [JsonProperty("id")]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("loyaltyId")]
        [JsonProperty("loyaltyId")]
        public string LoyaltyId { get; set; }

        [BsonElement("mobileNumber")]
        [JsonProperty("mobileNumber")]
        public string MobileNumber { get; set; }

        [BsonElement("points")]
        [JsonProperty("points")]
        public Point Point { get; set; }

        [BsonElement("cashback")]
        [JsonProperty("cashback")]
        public Cashback Cashback { get; set; }

        [BsonElement("vouchers")]
        [JsonProperty("vouchers")]
        public Voucher Voucher { get; set; }

        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonIgnoreIfNull]
        [BsonElement("createdOn")]
        [JsonProperty("createdOn")]
        public DateTime CreatedOn { get; set; }

        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonIgnoreIfNull]
        [BsonElement("updatedOn")]
        [JsonProperty("updatedOn")]
        public DateTime UpdatedOn { get; set; }

        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonIgnoreIfNull]
        [BsonElement("firstTransactionDate")]
        [JsonProperty("firstTransactionDate")]
        public DateTime? FirstTransactionDate { get; set; } = null;

        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonIgnoreIfNull]
        [BsonElement("lastTransactionDate")]
        [JsonProperty("lastTransactionDate")]
        public DateTime? LastTransactionDate { get; set; } = null;

        [BsonElement("uniqueCustomerCode")]
        [JsonProperty("uniqueCustomerCode")]
        public string UniqueCustomerCode { get; set; }
    }
}
