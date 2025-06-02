using Domain.Models.CampaignModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models.TransactionModel
{
    #region Common Transaction Section
    public class Transaction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("transactionId")]
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [BsonElement("mobileNumber")]
        [JsonProperty("mobileNumber")]
        public string MobileNumber { get; set; }

        [BsonElement("lob")]
        [JsonProperty("lob")]
        public string LOB { get; set; }

        [BsonElement("eventId")]
        [JsonProperty("eventId")]
        public string EventId { get; set; }

        [BsonElement("childEventCode")]
        [JsonProperty("childEventCode")]
        public string ChildEventCode { get; set; } = null;

        [BsonElement("channelCode")]
        [JsonProperty("channelCode")]
        public string ChannelCode { get; set; }

        [BsonElement("productCode")]
        [JsonProperty("productCode")]
        public string ProductCode { get; set; }

        [BsonElement("customerDetail")]
        [JsonProperty("customerDetail")]
        public Common.TransactionModel.CustomerDetail CustomerDetail { get; set; }

        [BsonElement("wallet")]
        [JsonProperty("wallet")]
        public Common.TransactionModel.Wallet Wallet { get; set; }

        [BsonElement("campaign")]
        [JsonProperty("campaign")]
        public Common.TransactionModel.Campaign Campaign { get; set; }

        [BsonElement("product")]
        [JsonProperty("product")]
        public List<Product> Products { get; set; }

        [BsonElement("utm")]
        [JsonProperty("utm")]
        public Common.TransactionModel.UTM UTM { get; set; }

        [BsonElement("transactionDetail")]
        [JsonProperty("transactionDetail")]
        public Common.TransactionModel.TransactionDetail TransactionDetail { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("createdDateTime")]
        [JsonProperty("createdDateTime")]
        public DateTime CreatedDateTime { get; set; } = DateTime.Now;

        [BsonElement("parentTransactionId")]
        [JsonProperty("parentTransactionId")]
        public string ParentTransactionId { get; set; } = null;

        [BsonElement("uniqueCustomerCode")]
        [JsonProperty("uniqueCustomerCode")]
        public string UniqueCustomerCode { get; set; }

        [BsonElement("emandate")]
        [JsonProperty("emandate")]
        public bool Emandate { get; set; } = false;

        [BsonIgnoreIfNull]
        [BsonElement("isTnCAccepted")]
        [JsonProperty("isTnCAccepted")]
        public bool IsTnCAccepted { get; set; }
    }
    #endregion

    #region Product block
    public class Product
    {
        [BsonElement("id")]
        [JsonProperty("id")]
        public string Id { get; set; } // ProductId

        [BsonElement("journey")]
        [JsonProperty("journey")]
        public string Journey { get; set; } // JourneyId

        [BsonElement("purchaseType")]
        [JsonProperty("purchaseType")]
        public string PurchaseType { get; set; } // Main, CrossSell

    }
    #endregion

    #region Domain Transcction Section
    public class ProcessedTransaction
    {
        [BsonElement("transactionRequest")]
        [JsonProperty("transactionRequest")]
        public Transaction TransactionRequest { get; set; }

        [BsonElement("customer")]
        [JsonProperty("customer")]
        public CustomerModel.Customer Customer { get; set; }

        [BsonElement("referrerCustomer")]
        [JsonProperty("referrerCustomer")]
        public CustomerModel.Customer ReferrerCustomer { get; set; }

        [BsonElement("matchedCampaigns")]
        [JsonProperty("matchedCampaigns")]
        public List<MatchedCampaign> MatchedCampaigns { get; set; } = new List<MatchedCampaign>();
    }
    public class MatchedCampaign
    {
        // TODO : add startdate and enddate field.
        [BsonElement("campaignId")]
        [JsonProperty("campaignId")]
        public string CampaignId { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("companyCode")]
        [JsonProperty("companyCode")]
        public string CompanyCode { get; set; }

        [BsonElement("eventType")]
        [JsonProperty("eventType")]
        public string EventType { get; set; } //"Wallet Creation|Wallet Load|Spent|Bill Payment", // from transaction payload

        [BsonElement("childEventCode")]
        [JsonProperty("childEventCode")]
        public string ChildEventCode { get; set; } = null; // Child Event Code For GenericActivity

        [BsonElement("offerType")]
        [JsonProperty("offerType")]
        public string OfferType { get; set; } // "Activity|Payment|Hybrid", // from matched campaign

        [BsonElement("isLock")]
        [JsonProperty("isLock")]
        public bool IsLock { get; set; }  //true,  // if event is matched with lock or unlock criteria
                                          //
        [BsonElement("isUnlock")]
        [JsonProperty("isUnLock")]
        public bool IsUnLock { get; set; }

        [BsonElement("isDirect")]
        [JsonProperty("isDirect")]
        public bool IsDirect { get; set; } //false, // if event is matched with direct criteria.

        //public List<string> RewardType { get; set; } //: "Points|Cashback|Voucher", // RewardType's value from RewardOption object

        [BsonElement("rewardCriteria")]
        [JsonProperty("rewardCriteria")]
        public CampaignModel.RewardCriteria RewardCriteria { get; set; } // From Campaign Object

        [BsonElement("rewardOptions")]
        [JsonProperty("rewardOptions")]
        public List<CampaignModel.RewardOptionType> RewardOptions { get; set; } // From Campaign Object

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("startDate")]
        [JsonProperty("startDate")]
        public DateTime StartDate { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("endDate")]
        [JsonProperty("endDate")]
        public DateTime? EndDate { get; set; }

        [BsonElement("narration")]
        [JsonProperty("narration")]
        public string Narration { get; set; }

        [BsonElement("ctaUrl")]
        [JsonProperty("ctaUrl")]
        public string CTAUrl { get; set; } = null;

        [BsonElement("isOncePerCampaign")]
        [JsonProperty("isOncePerCampaign")]
        public bool IsOncePerCampaign { get; set; } = false;

        [BsonElement("onceInLifeTime")]
        [JsonProperty("onceInLifeTime")]
        public OnceInLifeTime OnceInLifeTime { get; set; } = null;

        [BsonElement("unlockTermAndCondition")]
        [JsonProperty("unlockTermAndCondition")]
        public string UnlockTermAndCondition { get; set; }

        [BsonElement("isAssuredCashback")]
        [JsonProperty("isAssuredCashback")]
        public bool IsAssuredCashback { get; set; }

        [BsonElement("isAssuredPoints")]
        [JsonProperty("isAssuredPoints")]
        public bool IsAssuredPoints { get; set; }


        [BsonElement("isReferralProgram")]
        [JsonProperty("isReferralProgram")]
        public bool IsReferralProgram { get; set; }

        [BsonElement("referralRewardOptions")]
        [JsonProperty("referralRewardOptions")]
        public List<CampaignModel.RewardOptionType> ReferralRewardOptions { get; set; }

        [BsonElement("isAssuredCashbackKickOff")]
        [JsonProperty("isAssuredCashbackKickOff")]
        public bool IsAssuredCashbackKickOff { get; set; }

        [BsonElement("referalrewardNarration")]
        [JsonProperty("referalrewardNarration")]
        public string ReferalrewardNarration { get; set; }

        [BsonElement("referalUnlockTermAndCondition")]
        [JsonProperty("referalUnlockTermAndCondition")]
        public string ReferalUnlockTermAndCondition { get; set; }

        [BsonElement("ctaReferrerUrl")]
        [JsonProperty("ctaReferrerUrl")]
        public string CTAReferrerUrl { get; set; }

        [BsonElement("isReferralReward")]
        [JsonProperty("isReferralReward")]
        public bool IsReferralReward { get; set; } = false;

        [BsonElement("campaignStatus")]
        [JsonProperty("campaignStatus")]
        public string CampaignStatus { get; set; }

        [BsonElement("unlockAfterDate")]
        [JsonProperty("unlockAfterDate")]
        public DateTime? UnlockAfterDate { get; set; } = DateTime.Now;

        [BsonElement("isUnlockOnDuration")]
        [JsonProperty("isUnlockOnDuration")]
        public bool IsUnlockOnDuration { get; set; } = false;


        [BsonElement("onDemandExpiryNarration")]
        [JsonProperty("onDemandExpiryNarration")]
        public string OnDemandExpiryNarration { get; set; }

        [BsonElement("genericLockCard")]
        [JsonProperty("genericLockCard")]
        public CampaignModel.LockCard GenericLockCard { get; set; }

        [BsonElement("referralLockCard")]
        [JsonProperty("referralLockCard")]
        public CampaignModel.LockCard ReferralLockCard { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("responsysTemplateId")]
        [JsonProperty("responsysTemplateId")]
        public string ResponsysTemplateId { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("unlockResponsysTemplateId")]
        [JsonProperty("unlockResponsysTemplateId")]
        public string UnlockResponsysTemplateId { get; set; }

        [BsonElement("IsAssuredMultipleCurrency")]
        [JsonProperty("IsAssuredMultipleCurrency")]
        public bool IsAssuredMultipleCurrency { get; set; }

        [BsonElement("lob")]
        [JsonProperty("lob")]
        public string LOB { get; set; }

        [BsonElement("excludeFromBudgetCapping")]
        [JsonProperty("excludeFromBudgetCapping")]
        public bool ExcludeFromBudgetCapping { get; set; }

        // Slab Based 
        [BsonIgnoreIfNull]
        [BsonElement("isSlabBasedRewarding")]
        [JsonProperty("isSlabBasedRewarding")]
        public bool IsSlabBasedRewarding { get; set; }

        //[BsonElement("slabBasedRewarding")]
        //[JsonProperty("slabBasedRewarding")]
        //public SlabBasedRewarding SlabBasedRewarding { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("slabType")]
        [JsonProperty("slabType")]
        public string SlabType { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("noOfQualifiedTransactionsCount")]
        [JsonProperty("noOfQualifiedTransactionsCount")]
        public int NoOfQualifiedTransactionsCount { get; set; } = 0;

		[BsonElement("reverseStamping")]
		[JsonProperty("reverseStamping")]
		public bool ReverseStamping { get; set; }
        
        [BsonElement("email")]
		[JsonProperty("email")]
        public string Email { get; set; }
        
        [BsonElement("bflCampaignId")]
		[JsonProperty("bflCampaignId")]
        public string BflCampaignId { get; set; }
    }

    public class TransactionResponse
    {
        public string Key { get; set; }
        public Transaction TransactionRequest { get; set; }
    }
    #endregion    
}
