using Domain.Models.TransactionModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.CampaignModel
{

    #region Internal Classes
    public class CumulativeAndNonCumulativeCampaignResponse
    {
        public IEnumerable<CampaignModel.EarnCampaign> CumulativeCampaigns { get; set; }
        public IEnumerable<CampaignModel.EarnCampaign> NonCumulativeCampaigns { get; set; }
    }
    public class RejectedAndNonRejectedCampaignFilterResponse
    {
        public IEnumerable<CampaignModel.EarnCampaign> RejectedCampaigns { get; set; }
        public IEnumerable<CampaignModel.EarnCampaign> NonRejectedCampaigns { get; set; }
    }
    public class UnlockedCampaignCheckResponse
    {
        public bool IsUnlock { get; set; }
        public string IssuanceMode { get; set; }
        public string OfferType { get; set; }
    }
    public class UnlockEventDetailResponse
    {
        public string EventName { get; set; }
        public bool IsHybridUnlock { get; set; } = false;
        public CampaignModel.WalletLoad WalletLoad { get; set; } = null;
        public CampaignModel.WalletCreation WalletCreation { get; set; } = null;
        public CampaignModel.ClearBounceEMI ClearBounceEMI { get; set; } = null;
        public CampaignModel.CustomRewarded CustomRewarded { get; set; } = null;
        public CampaignModel.KYC KYC { get; set; } = null;
        public CampaignModel.Signup Signup { get; set; } = null;
        public CampaignModel.VPACreated VPACreated { get; set; } = null;
        public CampaignModel.MRNCreation MRNCreation { get; set; } = null;
        public CampaignModel.CDFinancing CDFinancing { get; set; } = null;
        //public CustomUpload CustomRewarding { get; set; }
    }
    public class DirectAndLockUnlockCampaignCheckResponse
    {
        public IEnumerable<CampaignModel.EarnCampaign> DirectCampaigns { get; set; }
        public IEnumerable<CampaignModel.EarnCampaign> LockUnlockCampaigns { get; set; }
    }
    #endregion


    #region Campaign Section

    #region Common
    [BsonIgnoreExtraElements]
    public class Document<T>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("id")]
        [JsonProperty("id")]
        public T Id { get; set; }

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

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("updatedAt")]
        [JsonProperty("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        [BsonElement("deletedBy")]
        [JsonProperty("deletedBy")]
        public string DeletedBy { get; set; }

        [BsonElement("isDeleted")]
        [JsonProperty("isDeleted")]
        public bool IsDeleted { get; set; }

        [BsonElement("isPublished")]
        [JsonProperty("isPublished")]
        public bool IsPublished { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("publishedAt")]
        [JsonProperty("publishedAt")]
        public DateTime? PublishedAt { get; set; }

        [BsonElement("versionId")]
        [JsonProperty("versionId")]
        public string VersionId { get; set; }

        [BsonElement("approver")]
        [JsonProperty("approver")]
        public string Approver { get; set; }

        [BsonElement("status")]
        [JsonProperty("status")]
        public string Status { get; set; } // Active, 

        [BsonElement("reasonTitle")]
        [JsonProperty("reasonTitle")]
        public string ReasonTitle { get; set; }


        [BsonElement("reasonDescription")]
        [JsonProperty("reasonDescription")]
        public string ReasonDescription { get; set; }

        [BsonElement("isDeployed")]
        [JsonProperty("isDeployed")]
        public int IsDeployed { get; set; } = 0;

        [BsonElement("oncePerCampaign")]
        [JsonProperty("oncePerCampaign")]
        public bool OncePerCampaign { get; set; }


        [BsonElement("onceInLifeTime")]
        [JsonProperty("onceInLifeTime")]
        public OnceInLifeTime OnceInLifeTime { get; set; }

    }
    #endregion

    #region Campaign
    [BsonIgnoreExtraElements]
    public class EarnCampaign : Document<string>
    {

        [BsonElement("campaignType")]
        [JsonProperty("campaignType")]
        public string CampaignType { get; set; }

        //>>>>>>>>>>>>>>>>>>>..General Campaign>>>>>>>>>>>>>>>>>>>>>>       

        [BsonIgnoreIfNull]
        [BsonElement("campaignName")]
        [JsonProperty("campaignName")]
        public string CampaignName { get; set; }

        [BsonIgnoreIfNull]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("startDate")]
        [JsonProperty("startDate")]
        public DateTime StartDate { get; set; }

        [BsonIgnoreIfNull]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("endDate")]
        [JsonProperty("endDate")]
        public DateTime? EndDate { get; set; }

        [BsonIgnoreIfNull]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("unLockExpiryDate")]
        [JsonProperty("unLockExpiryDate")]
        public DateTime? UnLockExpiryDate { get; set; } = null;

        [BsonIgnoreIfNull]
        [BsonElement("bflCampaignId")]
        [JsonProperty("bflCampaignId")]
        public string BFLCampaignId { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("offerType")]
        [JsonProperty("offerType")]
        public string OfferType { get; set; } //Generic, Triple Reward

        [BsonIgnoreIfNull]
        [BsonElement("lob")]
        [JsonProperty("lob")]
        public string LOB { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("companyCode")]
        [JsonProperty("companyCode")]
        public string CompanyCode { get; set; }//Company 

        [BsonIgnoreIfNull]
        [BsonElement("customerType")]
        [JsonProperty("customerType")]
        public List<string> CustomerType { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("customerStatus")]
        [JsonProperty("customerStatus")]
        public List<string> CustomerStatus { get; set; }

        [BsonElement("rewardCategory")]
        [JsonProperty("rewardCategory")]
        public string RewardCategory { get; set; }

        [BsonElement("channel")]
        [JsonProperty("channel")]
        public List<string> Channel { get; set; }

        [BsonElement("installationSource")]
        [JsonProperty("installationSource")]
        public InstallationSource InstallationSource { get; set; }

        [BsonElement("segmentSelect")]
        [JsonProperty("segmentSelect")]
        public string SegmentSelect { get; set; }

        [BsonElement("segmentType")]
        [JsonProperty("segmentType")]
        public string SegmentType { get; set; }

        [BsonElement("segment")]
        [JsonProperty("segment")]
        public List<string> Segment { get; set; }

        [BsonElement("rmsAttributes")]
        [JsonProperty("rmsAttributes")]
        public List<RMSAttribute> RmsAttributes { get; set; }

        [BsonElement("filter")]
        [JsonProperty("filter")]
        public Filter Filter { get; set; }

        //>>>>>>>>>>>>>>>>>>>Reward Criteria Campaign>>>>>>>>>>>>>>>>>>>>>>
        [BsonElement("rewardCriteria")]
        [JsonProperty("rewardCriteria")]
        public RewardCriteria RewardCriteria { get; set; }


        //>>>>>>>>>>>>>>>>>>>Reward option>>>>>>>>>>>>>>>>>>>>>>

        [BsonElement("rewardOption")]
        [JsonProperty("rewardOption")]
        public List<RewardOptionType> RewardOption { get; set; }

        //>>>>>>>>>>>>>>>>>>>Content Tab>>>>>>>>>>>>>>>>>>>>>>

        [BsonElement("content")]
        [JsonProperty("content")]
        public Content Content { get; set; }

        //>>>>>>>>>>>>>>>>>>>Alert Tab>>>>>>>>>>>>>>>>>>>>>>
        [BsonElement("alert")]
        [JsonProperty("alert")]
        public Alert Alert { get; set; }

        //>>>>>>>>>>>>>>>>>Membership Tab>>>>>>>>>>>>>>>>>>>>
        [BsonElement("membershipReward")]
        [JsonProperty("membershipReward")]
        public MembershipReward MembershipReward { get; set; }

        [BsonElement("isAnySubscriptionType")]
        [JsonProperty("isAnySubscriptionType")]
        public bool IsAnySubscriptionType { get; set; } = true;

        [BsonElement("subscriptionTypes")]
        [JsonProperty("subscriptionTypes")]
        public List<string> SubscriptionTypes { get; set; } = new List<string>();

        [BsonElement("excludeFromBudgetCapping")]
        [JsonProperty("excludeFromBudgetCapping")]
        public bool ExcludeFromBudgetCapping { get; set; }

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
    }

    #endregion

    [BsonIgnoreExtraElements]
    public class OnceInLifeTime
    {
        [BsonElement("value")]
        [JsonProperty("value")]
        public bool Value { get; set; }

        [BsonElement("tags")]
        [JsonProperty("tags")]
        public List<string> Tags { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class InstallationSource
    {
        [BsonElement("source")]
        [JsonProperty("source")]
        public List<string> Source { get; set; }

        [BsonElement("anyInstallationSource")]
        [JsonProperty("anyInstallationSource")]
        public bool AnyInstallationSource { get; set; }
    }

    #region Filter Campaign
    [BsonIgnoreExtraElements]
    public class Filter
    {

        [BsonElement("eventCode")]
        [JsonProperty("eventCode")]
        public List<string> EventCode { get; set; } = new List<string>();

        [BsonElement("isDirect")]
        [JsonProperty("isDirect")]
        public bool IsDirect { get; set; } = false;

        [BsonElement("directEvent")]
        [JsonProperty("directEvent")]
        public string DirectEvent { get; set; } = String.Empty;

        [BsonElement("isLock")]
        [JsonProperty("isLock")]
        public bool IsLock { get; set; } = false;

        [BsonElement("lockEvent")]
        [JsonProperty("lockEvent")]
        public string LockEvent { get; set; } = String.Empty;

        [BsonElement("isUnlock")]
        [JsonProperty("isUnlock")]
        public bool IsUnlock { get; set; } = false;

        [BsonElement("unlockEvent")]
        [JsonProperty("unlockEvent")]
        public string UnlockEvent { get; set; } = String.Empty;

        [BsonElement("isTripleReward")]
        [JsonProperty("isTripleReward")]
        public bool IsTripleReward { get; set; } = false;

        [BsonElement("isCumulative")]
        [JsonProperty("isCumulative")]
        public bool IsCumulative { get; set; } = false;

        [BsonElement("isAssuredCashback")]
        [JsonProperty("isAssuredCashback")]
        public bool IsAssuredCashback { get; set; }

        [BsonElement("isAssuredPoints")]
        [JsonProperty("isAssuredPoints")]
        public bool IsAssuredPoints { get; set; }

        [BsonElement("isAssuredCashbackKickOff")]
        [JsonProperty("isAssuredCashbackKickOff")]
        public bool IsAssuredCashbackKickOff { get; set; }

        [BsonElement("isRefferalProgram")]
        [JsonProperty("isRefferalProgram")]
        public bool IsRefferalProgram { get; set; }

        [BsonElement("isUnlockOnDuration")]
        [JsonProperty("isUnlockOnDuration")]
        public bool IsUnlockOnDuration { get; set; }

        [BsonElement("isLending")]
        [JsonProperty("isLending")]
        public bool IsLending { get; set; }
        [BsonElement("isSubscription")]
        [JsonProperty("isSubscription")]
        public bool IsSubscription { get; set; }

        [BsonElement("subscriptionId")]
        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }
        [BsonElement("isBundle")]
        [JsonProperty("isBundle")]
        public bool IsBundle { get; set; }
        [BsonElement("bundleId")]
        [JsonProperty("bundleId")]
        public string BundleId { get; set; }

        [BsonElement("isMembershipReward")]
        [JsonProperty("isMembershipReward")]
        public bool IsMembershipReward { get; set; }

        [BsonElement("isAnySubscriptionType")]
        [JsonProperty("isAnySubscriptionType")]
        public bool IsAnySubscriptionType { get; set; } = true;

        [BsonElement("subscriptionTypes")]
        [JsonProperty("subscriptionTypes")]
        public List<string> SubscriptionTypes { get; set; } = new List<string>();

        [BsonElement("isAssuredMultiCurrency")]
        [JsonProperty("isAssuredMultiCurrency")]
        public bool IsAssuredMultiCurrency { get; set; } = true;
    }

    #endregion

    #region RMS
    [BsonIgnoreExtraElements]
    public class RMSAttribute
    {
        [BsonElement("attributeType")]
        [JsonProperty("attributeType")]
        public string AttributeType { get; set; } //points || cashback || subscription

        [BsonElement("parameter")]
        [JsonProperty("parameter")]
        public string Parameter { get; set; } //available balance || lifetime earn || subscription tier

        [BsonElement("parameterCode")]
        [JsonProperty("parameterCode")]
        public string parameterCode { get; set; }

        [BsonElement("startRange")]
        [JsonProperty("startRange")]
        public int StartRange { get; set; }

        [BsonElement("endRange")]
        [JsonProperty("endRange")]
        public int EndRange { get; set; }

        [BsonElement("subscriptionTiers")]
        [JsonProperty("subscriptionTiers")]
        public List<SubscriptionTier> SubscriptionTiers { get; set; } = new List<SubscriptionTier>();
    }

    [BsonIgnoreExtraElements]
    public class SubscriptionTier
    {
        [BsonIgnoreIfNull]
        [BsonElement("tierId")]
        [JsonProperty("tierId")]
        public int TierId { get; set; }
        [BsonIgnoreIfNull]
        [BsonElement("tierName")]
        [JsonProperty("tierName")]
        public string TierName { get; set; } = string.Empty;
        [BsonIgnoreIfNull]
        [BsonElement("tierRank")]
        [JsonProperty("tierRank")]
        public int TierRank { get; set; }
        [BsonIgnoreIfNull]
        [BsonElement("isPrimary")]
        [JsonProperty("isPrimary")]
        public bool IsPrimary { get; set; }
    }

    #endregion

    #region RewardCriteria origin

    #region RewardCriteria
    [BsonIgnoreExtraElements]
    public class RewardCriteria
    {
        [BsonElement("rewardIssuance")]
        [JsonProperty("rewardIssuance")]
        public string RewardIssuance { get; set; } //Direct, Withlockandunlock

        [BsonElement("offerEventDirect")]
        [JsonProperty("offerEventDirect")]
        public string OfferEventDirect { get; set; }

        [BsonElement("direct")]
        [JsonProperty("direct")]
        public DirectEvent Direct { get; set; }

        [BsonElement("offerEventLock")]
        [JsonProperty("offerEventLock")]
        public string OfferEventLock { get; set; }

        [BsonElement("offerEventUnlock")]
        [JsonProperty("offerEventUnlock")]
        public string OfferEventUnlock { get; set; }

        [BsonElement("withLockUnlock")]
        [JsonProperty("withLockUnlock")]
        public WithLockUnlock WithLockUnlock { get; set; }

        [BsonElement("asScratchCard")]
        [JsonProperty("asScratchCard")]
        public AsScratchCard AsScratchCard { get; set; }

        [BsonElement("refferalProgram")]
        [JsonProperty("refferalProgram")]
        public RefferalProgram RefferalProgram { get; set; }

    }

    public interface ICommonEvent { }

    [BsonKnownTypes(typeof(PaymentDirect), typeof(CustomUpload), typeof(CDFinancing), typeof(MRNCreation), typeof(DOISSUE), typeof(DICOMPLETED), typeof(Signup), typeof(ClearBounceEMI), typeof(CustomRewarded), typeof(KYC), typeof(GenericActivity), typeof(VPACreated), typeof(WalletCreation), typeof(WalletLoad), typeof(EMandateCreation), typeof(UPILite), typeof(Any), typeof(EMIREPAYMENT))]
    public class CommonEvent : ICommonEvent { }

    public interface ICommonEventType { }
    public class CommonEventType : ICommonEventType
    {
        [BsonElement("eventName")]
        [JsonProperty("eventName")]
        public string EventName { get; set; }

        [BsonElement("eventType")]
        [JsonProperty("eventType")]
        public string EventType { get; set; }

        [BsonElement("event")]
        [JsonProperty("event")]
        public CommonEvent Event { get; set; }
    }

    public class DirectEvent : CommonEventType
    {
        [BsonElement("isCrossSellApplicable")]
        [JsonProperty("isCrossSellApplicable")]
        public bool IsCrossSellApplicable { get; set; }

        // For Main & Cross Sell type
        [BsonIgnoreIfNull]
        [BsonElement("products")]
        [JsonProperty("products")]
        public List<Product> Products { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Product
    {
        [BsonIgnoreIfNull]
        [BsonElement("productId")]
        [JsonProperty("productId")]
        public List<string> ProductId { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("journey")]
        [JsonProperty("journey")]
        public List<string> Journey { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("purchaseType")]
        [JsonProperty("purchaseType")]
        public string PurchaseType { get; set; }
    }

    public class LockEvent : CommonEventType
    {
        [BsonElement("noOfDays")]
        [JsonProperty("noOfDays")]
        public int NoOfDays { get; set; } = 0; //if isLockExpire

        [BsonElement("isCrossSellApplicable")]
        [JsonProperty("isCrossSellApplicable")]
        public bool IsCrossSellApplicable { get; set; }

        // For Main & Cross Sell type
        [BsonIgnoreIfNull]
        [BsonElement("productLock")]
        [JsonProperty("productLock")]
        public List<Product> ProductLock { get; set; }
    }
    public class UnlockEvent : CommonEventType
    {
        [BsonElement("isLockExpire")]
        [JsonProperty("isLockExpire")]
        public bool IsLockExpire { get; set; }

        // Values would be event or duration
        [BsonIgnoreIfNull]
        [BsonElement("unlockBasedOn")]
        [JsonProperty("unlockBasedOn")]
        public string UnlockBasedOn { get; set; }

        //No. of days
        [BsonIgnoreIfNull]
        [BsonElement("unlockDuration")]
        [JsonProperty("unlockDuration")]
        public int UnlockDuration { get; set; } = 0;

        [BsonElement("isCrossSellApplicable")]
        [JsonProperty("isCrossSellApplicable")]
        public bool IsCrossSellApplicable { get; set; }

        // For Main & Cross Sell type
        [BsonIgnoreIfNull]
        [BsonElement("productUnlock")]
        [JsonProperty("productUnlock")]
        public List<Product> ProductUnlock { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class WithLockUnlock
    {
        [BsonElement("lockEvent")]
        [JsonProperty("lockEvent")]
        public LockEvent LockEvent { get; set; } //Activity,Payment,Lending
                                                 //
        [BsonElement("unlockEvent")]
        [JsonProperty("unlockEvent")]
        public UnlockEvent UnlockEvent { get; set; } //Activity,Payment,Lending
    }

    [BsonIgnoreExtraElements]
    public class OfferEvent : CommonPaymentObject
    {
        [BsonElement("activityEvent")]
        [JsonProperty("activityEvent")]
        public ActivityEvent Activity { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("payment")]
        [JsonProperty("payment")]
        public PaymentDirect Payment { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("lending")]
        [JsonProperty("lending")]
        public ActivityEvent Lending { get; set; }
    }
    #endregion

    #region ReferralProgram

    public class RefferalProgram
    {
        [BsonElement("rewardOptionType")]
        [JsonProperty("rewardOptionType")]
        public RewardOptionType RewardOptionType { get; set; }


        [BsonElement("capping")]
        [JsonProperty("capping")]
        public int Capping { get; set; }
    }

    #endregion

    #region Direct
    [BsonIgnoreExtraElements]
    public class Direct
    {
        //activity dirct
        [BsonIgnoreIfNull]
        [BsonElement("activityDirect")]
        [JsonProperty("activityDirect")]
        public ActivityDirect ActivityDirect { get; set; }

        //payment direct
        [BsonIgnoreIfNull]
        [BsonElement("paymentDirect")]
        [JsonProperty("paymentDirect")]
        public PaymentDirect PaymentDirect { get; set; }

        //payment direct
        [BsonIgnoreIfNull]
        [BsonElement("lendingDirect")]
        [JsonProperty("lendingDirect")]
        public LendingDirect LendingDirect { get; set; }
    }
    #endregion

    #region WithLockUnlock
    //[BsonIgnoreExtraElements]
    //public class WithLockUnlock
    //{
    //    //Activity withLockUnlock
    //    [BsonIgnoreIfNull]
    //    [BsonElement("activityWithLockUnlock")]
    //    [JsonProperty("activityWithLockUnlock")]
    //    public ActivityWithLockUnlock ActivityWithLockUnlock { get; set; }

    //    //hybrid withLockUnlock
    //    [BsonIgnoreIfNull]
    //    [BsonElement("hybridWithLockUnlock")]
    //    [JsonProperty("hybridWithLockUnlock")]
    //    public HybridWithLockUnlock HybridWithLockUnlock { get; set; }

    //    [BsonIgnoreIfNull]
    //    [BsonElement("paymentHybridWithLockUnlock")]
    //    [JsonProperty("paymentHybridWithLockUnlock")]
    //    public PaymentHybridWithLockUnlock PaymentHybridWithLockUnlock { get; set; }

    //    [BsonIgnoreIfNull]
    //    [BsonElement("lendingWithLockUnlock")]
    //    [JsonProperty("lendingWithLockUnlock")]
    //    public LendingWithLockUnlock LendingWithLockUnlock { get; set; }
    //}
    #endregion

    [BsonIgnoreExtraElements]

    public class PaymentHybridWithLockUnlock : CommonPaymentObject
    {
        [BsonElement("lockEvent")]
        [JsonProperty("lockEvent")]
        public PaymentDirect LockEvent { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("unLockEvent")]
        [JsonProperty("unLockEvent")]
        public PaymentDirect UnLockEvent { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class SlabBasedRewarding
    {
        [BsonElement("isMaxTransactionPerCustomer")]
        [JsonProperty("isMaxTransactionPerCustomer")]
        public bool IsMaxTransactionPerCustomer { get; set; }

        [BsonElement("noOfTransaction")]
        [JsonProperty("noOfTransaction")]
        public string NoOfTransaction { get; set; }
    }

    public class CommonPaymentObject
    {
        [BsonElement("isLockExpire")]
        [JsonProperty("isLockExpire")]
        public bool IsLockExpire { get; set; }

        [BsonElement("noOfDays")]
        [JsonProperty("noOfDays")]
        public int NoOfDays { get; set; } = 0; //if isLockExpire      

        // Values would be event or duration
        [BsonIgnoreIfNull]
        [BsonElement("unlockBasedOn")]
        [JsonProperty("unlockBasedOn")]
        public string UnlockBasedOn { get; set; }

        //No. of days
        [BsonIgnoreIfNull]
        [BsonElement("unlockDuration")]
        [JsonProperty("unlockDuration")]
        public int UnlockDuration { get; set; } = 0;
    }

    #region AsScratchCard
    [BsonIgnoreExtraElements]
    public class AsScratchCard
    {
        [BsonElement("transactionType")]
        [JsonProperty("transactionType")]
        public string TransactionType { get; set; }

        [BsonElement("duration")]
        [JsonProperty("duration")]
        public Duration Duration { get; set; }
    }
    #endregion

    #region Duration
    public class Duration
    {
        [BsonElement("type")]
        [JsonProperty("type")]
        public string Type { get; set; }

        [BsonElement("recurrence")]
        [JsonProperty("recurrence")]
        public string Recurrence { get; set; }

        [BsonElement("paymentCategories")]
        [JsonProperty("paymentCategories")]
        public List<string> PaymentCategories { get; set; }

        [BsonElement("paymentInstruments")]
        [JsonProperty("paymentInstruments")]
        public List<string> PaymentInstruments { get; set; }

        [BsonElement("isMinTransaction")]
        [JsonProperty("isMinTransaction")]
        public bool IsMinTransaction { get; set; }

        [BsonElement("minTransactionAmount")]
        [JsonProperty("minTransactionAmount")]
        public int MinTransactionAmount { get; set; }

        [BsonElement("transactionCount")]
        [JsonProperty("transactionCount")]
        public int TransactionCount { get; set; }


        [BsonElement("merchant")]
        [JsonProperty("merchant")]
        public Merchant Merchant { get; set; }
    }
    #endregion

    #region Merchant Triple Reward
    public class Merchant
    {

        [BsonElement("merchantCategory")]
        [JsonProperty("merchantCategory")]
        public List<string> MerchantCategory { get; set; }

        [BsonElement("groupMerchantId")]
        [JsonProperty("groupMerchantId")]
        public List<string> GroupMerchantId { get; set; }

        [BsonElement("merchantId")]
        [JsonProperty("merchantId")]
        public List<string> MerchantId { get; set; }

        [BsonElement("merchantSegment")]
        [JsonProperty("merchantSegment")]
        public string MerchantSegment { get; set; }

        [BsonElement("mode")]
        [JsonProperty("mode")]
        public string Mode { get; set; }

        [BsonElement("segment")]
        [JsonProperty("segment")]
        public MerchantSegment Segment { get; set; }

        [BsonElement("merchantType")]
        [JsonProperty("merchantType")]
        public string MerchantType { get; set; } = "p2m";
    }
    #endregion

    #region Segment Triple Reward
    public class MerchantSegment
    {
        [BsonElement("segmentCode")]
        [JsonProperty("segmentCode")]
        public List<string> SegmentCode { get; set; }

        [BsonElement("isIncluded")]
        [JsonProperty("isIncluded")]
        public bool IsIncluded { get; set; }
    }
    #endregion

    #region ActiveDirect
    public class ActivityDirect
    {
        [BsonElement("event")]
        [JsonProperty("event")]
        public ActivityEvent Event { get; set; }
    }
    #endregion

    #region ActiveWithLockUnlock
    public class ActivityWithLockUnlock : CommonPaymentObject
    {
        [BsonElement("lockEvent")]
        [JsonProperty("lockEvent")]
        public ActivityEvent LockEvent { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("unLockEvent")]
        [JsonProperty("unLockEvent")]
        public ActivityEvent UnLockEvent { get; set; }
    }
    #endregion

    #region LendingDirect
    public class LendingDirect
    {
        [BsonElement("lendingEvent")]
        [JsonProperty("lendingEvent")]
        public LendingEvent LendingEvent { get; set; }
    }
    #endregion

    #region LendingWithLockUnlock
    public class LendingWithLockUnlock : CommonPaymentObject
    {
        [BsonElement("lockEvent")]
        [JsonProperty("lockEvent")]
        public LendingEvent LockEvent { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("unLockEvent")]
        [JsonProperty("unLockEvent")]
        public LendingEvent UnLockEvent { get; set; }
    }
    #endregion

    #region PaymentDirect
    [BsonIgnoreExtraElements]
    public class PaymentDirect : CommonEvent
    {

        [BsonElement("transactionType")]
        [JsonProperty("transactionType")]
        public string TransactionType { get; set; }    //single,cumulative and any

        [BsonElement("single")]
        [JsonProperty("single")]
        public Single Single { get; set; }

        [BsonElement("cumulative")]
        [JsonProperty("cumulative")]
        public Cumulative Cumulative { get; set; }


        [BsonElement("paymentCategories")]
        [JsonProperty("paymentCategories")]
        public List<string> PaymentCategories { get; set; }

        [BsonElement("paymentInstruments")]
        [JsonProperty("paymentInstruments")]
        public List<string> PaymentInstruments { get; set; }

        [BsonElement("merchant")]
        [JsonProperty("merchant")]
        public Merchant Merchant { get; set; }


        [BsonElement("bbps")]
        [JsonProperty("bbps")]
        public BBPS BBPS { get; set; }

        [BsonElement("vpaMerchantSegment")]
        [JsonProperty("vpaMerchantSegment")]
        public VpaSegmentOnOff VpaMerchantSegment { get; set; }


        [BsonElement("emandate")]
        [JsonProperty("emandate")]
        public bool Emandate { get; set; } = false;

        [BsonElement("oncePerDestinationVPA")]
        [JsonProperty("oncePerDestinationVPA")]
        public bool OncePerDestinationVPA { get; set; }

        [BsonElement("subscriptionId")]
        [JsonProperty("subscriptionId")]
        public List<string> SubscriptionId { get; set; }
    }
    #endregion

    #region BBPS
    public class BBPS
    {
        [BsonElement("anyCategory")]
        [JsonProperty("anyCategory")]
        public bool AnyCategory { get; set; }

        [BsonElement("billerCategories")]
        [JsonProperty("billerCategories")]
        public List<BBPSBillerCategory> BillerCategories { get; set; }
    }
    #endregion

    #region
    public class BBPSBillerCategory
    {
        [BsonElement("biller")]
        [JsonProperty("biller")]
        public string Biller { get; set; }

        [BsonElement("billerCategory")]
        [JsonProperty("billerCategory")]
        public string BillerCategory { get; set; }

    }
    #endregion


    #region HybridWithLockUnlock
    [BsonIgnoreExtraElements]
    public class HybridWithLockUnlock : CommonPaymentObject
    {

        //lock events 
        [BsonElement("lockActivity")]
        [JsonProperty("lockActivity")]
        public ActivityEvent LockActivity { get; set; }

        //unlock events
        //hybrid payment
        [BsonIgnoreIfNull]
        [BsonElement("unlockPayment")]
        [JsonProperty("unlockPayment")]
        public PaymentDirect UnlockPayment { get; set; }

        [BsonElement("transactionType")]
        [JsonProperty("transactionType")]
        public string TransactionType { get; set; }    //single,cumulative and any

        [BsonElement("single")]
        [JsonProperty("single")]
        public Single Single { get; set; }

        [BsonElement("cumulative")]
        [JsonProperty("cumulative")]
        public Cumulative Cumulative { get; set; }


        [BsonElement("paymentCategories")]
        [JsonProperty("paymentCategories")]
        public List<string> PaymentCategories { get; set; }

        [BsonElement("paymentInstruments")]
        [JsonProperty("paymentInstruments")]
        public List<string> PaymentInstruments { get; set; }

        [BsonElement("merchant")]
        [JsonProperty("merchant")]
        public Merchant Merchant { get; set; }


        [BsonElement("bbps")]
        [JsonProperty("bbps")]
        public BBPS BBPS { get; set; }

        [BsonElement("vpaMerchantSegment")]
        [JsonProperty("vpaMerchantSegment")]
        public VpaSegmentOnOff VpaMerchantSegment { get; set; }
    }
    #endregion

    #region ActivityEvent
    [BsonIgnoreExtraElements]
    public class ActivityEvent
    {
        [BsonElement("eventName")]
        [JsonProperty("eventName")]
        public string EventName { get; set; }
        [BsonElement("any")]
        [JsonProperty("any")]
        [BsonIgnoreIfNull]
        public Any Any { get; set; }

        [BsonElement("clearBounceEMI")]
        [JsonProperty("clearBounceEMI")]
        [BsonIgnoreIfNull]

        public ClearBounceEMI ClearBounceEMI { get; set; }
        [BsonElement("customRewarded")]
        [JsonProperty("customRewarded")]
        [BsonIgnoreIfNull]

        public CustomRewarded CustomRewarded { get; set; }
        [BsonElement("kyc")]
        [JsonProperty("kyc")]
        [BsonIgnoreIfNull]

        public KYC KYC { get; set; }
        [BsonElement("signup")]
        [JsonProperty("signup")]
        [BsonIgnoreIfNull]

        public Signup Signup { get; set; }
        [BsonElement("vpaCreated")]
        [JsonProperty("vpaCreated")]
        [BsonIgnoreIfNull]

        public VPACreated VPACreated { get; set; }
        [BsonElement("walletCreation")]
        [JsonProperty("walletCreation")]
        [BsonIgnoreIfNull]

        public WalletCreation WalletCreation { get; set; }

        [BsonElement("walletLoad")]
        [JsonProperty("walletLoad")]
        [BsonIgnoreIfNull]
        public WalletLoad WalletLoad { get; set; }

        [BsonElement("mrnCreation")]
        [JsonProperty("mrnCreation")]
        [BsonIgnoreIfNull]
        public MRNCreation MRNCreation { get; set; }

        [BsonElement("cdFinancing")]
        [JsonProperty("cdFinancing")]
        [BsonIgnoreIfNull]
        public CDFinancing CDFinancing { get; set; }

        [BsonElement("customUpload")]
        [JsonProperty("customUpload")]
        [BsonIgnoreIfNull]
        public CustomUpload CustomUpload { get; set; }

        [BsonElement("emiRepayment")]
        [JsonProperty("emiRepayment")]
        [BsonIgnoreIfNull]
        public EMIREPAYMENT EMIRepayment { get; set; }

        [BsonElement("doIssue")]
        [JsonProperty("doIssue")]
        [BsonIgnoreIfNull]
        public DOISSUE DOIssue { get; set; }

        [BsonElement("diCompleted")]
        [JsonProperty("diCompleted")]
        [BsonIgnoreIfNull]
        public DICOMPLETED DICompleted { get; set; }

        [BsonElement("genericActivity")]
        [JsonProperty("genericActivity")]
        [BsonIgnoreIfNull]
        public GenericActivity GenericActivity { get; set; }

        [BsonElement("eMandateCreation")]
        [JsonProperty("eMandateCreation")]
        [BsonIgnoreIfNull]
        public EMandateCreation EMandateCreation { get; set; }

    }
    #endregion

    #region LendingEvent
    [BsonIgnoreExtraElements]
    public class LendingEvent
    {
        [BsonElement("eventName")]
        [JsonProperty("eventName")]
        public string EventName { get; set; }

        [BsonElement("doIssue")]
        [JsonProperty("doIssue")]
        [BsonIgnoreIfNull]
        public DOISSUE DOISSUE { get; set; }

        [BsonElement("diCompleted")]
        [JsonProperty("diCompleted")]
        [BsonIgnoreIfNull]
        public DICOMPLETED DICOMPLETED { get; set; }

        [BsonElement("genericActivity")]
        [JsonProperty("genericActivity")]
        [BsonIgnoreIfNull]
        public GenericActivity GenericActivity { get; set; }

    }
    #endregion

    #region Events
    public class CustomUpload : CommonEvent { }

    public class CDFinancing : CommonEvent { }

    public class MRNCreation : CommonEvent { }

    public class Signup : CommonEvent
    {

    }
    public class ClearBounceEMI : CommonEvent
    {
        [BsonElement("bounceEMIPaid")]
        [JsonProperty("bounceEMIPaid")]
        public bool BounceEMIPaid { get; set; }

        [BsonElement("emiAmount")]
        [JsonProperty("emiAmount")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal EMIAmount { get; set; }
    }
    public class CustomRewarded : CommonEvent
    {
        [BsonElement("file")]
        [JsonProperty("file")]
        public byte[] File { get; set; }

        [BsonElement("fileName")]
        [JsonProperty("fileName")]
        public string FileName { get; set; }
    }
    public class KYC : CommonEvent
    {
        [BsonElement("completionType")]
        [JsonProperty("completionType")]
        public string CompletionType { get; set; }
    }

    public class VPACreated : CommonEvent
    {
        [BsonElement("vpa")]
        [JsonProperty("vpa")]
        public string VPA { get; set; }
    }
    public class WalletCreation : CommonEvent
    {

    }
    public class WalletLoad : CommonEvent
    {
        [BsonElement("loadCount")]
        [JsonProperty("loadCount")]
        public int LoadCount { get; set; }

        [BsonElement("paymentInstruments")]
        [JsonProperty("paymentInstruments")]
        public List<string> PaymentInstruments { get; set; }

        [BsonElement("isMinimumLoadAmount")]
        [JsonProperty("isMinimumLoadAmount")]
        public bool IsMinimumLoadAmount { get; set; }

        [BsonElement("minimumLoadAmount")]
        [JsonProperty("minimumLoadAmount")]
        public decimal MinimumLoadAmount { get; set; } //if IsMinimumLoadAmount=true

        [BsonElement("isLoadInterval")]
        [JsonProperty("isLoadInterval")]
        public bool IsLoadInterval { get; set; }

        [BsonElement("loadIntervalHours")]
        [JsonProperty("loadIntervalHours")]
        public int LoadIntervalHours { get; set; } //if LoadInterval=true

        [BsonElement("loadIntervalMinutes")]
        [JsonProperty("loadIntervalMinutes")]
        public int LoadIntervalMinutes { get; set; } //if LoadInterval=true
    }

    [BsonIgnoreExtraElements]
    public class UPILite : CommonEvent
    {
        [BsonElement("isLoadCount")]
        [JsonProperty("isLoadCount")]
        public bool IsLoadCount { get; set; }
        [BsonElement("loadCount")]
        [JsonProperty("loadCount")]
        public int LoadCount { get; set; }

        [BsonElement("countType")]
        [JsonProperty("countType")]
        public int CountType { get; set; }
        [BsonElement("paymentInstruments")]
        [JsonProperty("paymentInstruments")]
        public string PaymentInstruments { get; set; }
        [BsonElement("isMinimumAmount")]
        [JsonProperty("isMinimumAmount")]
        public bool IsMinimumAmount { get; set; }
        [BsonElement("minimumAmount")]
        [JsonProperty("minimumAmount")]
        public decimal MinimumAmount { get; set; } //if IsMinimumAmount=true
    }

    public class DOISSUE : CommonEvent
    {
        [BsonElement("isLoanAmountApplicable ")]
        [JsonProperty("isLoanAmountApplicable ")]
        public bool IsLoanAmountApplicable { get; set; }

        [BsonElement("loanAmount")]
        [JsonProperty("loanAmount")]
        public decimal LoanAmount { get; set; }
    }
    public class DICOMPLETED : CommonEvent
    {
        [BsonElement("isLoanAmountApplicable ")]
        [JsonProperty("isLoanAmountApplicable ")]
        public bool IsLoanAmountApplicable { get; set; }

        [BsonElement("loanAmount")]
        [JsonProperty("loanAmount")]
        public decimal LoanAmount { get; set; }
    }
    public class EMIREPAYMENT : CommonEvent { }
    public class GenericActivity : CommonEvent
    {
        [BsonElement("childEvent")]
        [JsonProperty("childEvent")]
        public string ChildEventCode { get; set; }
    }

    public class EMandateCreation : CommonEvent
    {
        [BsonElement("vpaSegment")]
        [JsonProperty("vpaSegment")]
        public VpaSegmentOnOff VpaSegment { get; set; }
    }

    public class Any : CommonEvent
    {

    }
    #endregion


    #region Transaction single and cumulative
    public class Single
    {

        [BsonElement("transactionCount")]
        [JsonProperty("transactionCount")]
        public int TransactionCount { get; set; }

        [BsonElement("transactionCountType")]
        [JsonProperty("transactionCountType")]
        public int TransactionCountType { get; set; }

        [BsonElement("isTransactionCount")]
        [JsonProperty("isTransactionCount")]
        public bool isTransactionCount { get; set; }

        [BsonElement("isTransactionAmount")]
        [JsonProperty("isTransactionAmount")]
        public bool IsTransactionAmount { get; set; }

        [BsonElement("minTransactionAmount")]
        [JsonProperty("minTransactionAmount")]
        public decimal? MinTransactionAmount { get; set; }

    }

    public class SpendOccurence
    {
        [BsonElement("type")]
        [JsonProperty("type")]
        public string Type { get; set; } //single || multiple

        [BsonElement("single")]
        [JsonProperty("single")]
        public SingleSpendOccurence Single { get; set; }

        [BsonElement("multiple")]
        [JsonProperty("multiple")]
        public MultipleSpendOccurance Multiple { get; set; }
    }
    public class SingleSpendOccurence
    {
        [BsonElement("type")]
        [JsonProperty("type")]
        public string Type { get; set; } = "Single";

        [BsonElement("recurrence")]
        [JsonProperty("recurrence")]
        public string Recurrence { get; set; }
    }
    public class MultipleSpendOccurance
    {
        [BsonElement("type")]
        [JsonProperty("type")]
        public string Type { get; set; } = "Multiple";

        [BsonElement("occurence")]
        [JsonProperty("occurence")]
        public int Occurence { get; set; }

        [BsonElement("recurrence")]
        [JsonProperty("recurrence")]
        public string Recurrence { get; set; }

        [BsonElement("spendType")]
        [JsonProperty("spendType")]
        public string SpendType { get; set; }
    }
    public class SpendBy
    {
        [BsonElement("type")]
        [JsonProperty("type")]
        public string Type { get; set; } //fixed || top spender

        [BsonElement("fixedSpendBy")]
        [JsonProperty("fixedSpendBy")]
        public FixedSpendBy FixedSpendBy { get; set; }

        [BsonElement("topSpenderSpendBy")]
        [JsonProperty("topSpenderSpendBy")]
        public TopSpenderSpendBy TopSpenderSpendBy { get; set; }

    }
    public class FixedSpendBy
    {
        [BsonElement("mode")]
        [JsonProperty("mode")]
        public string Mode { get; set; }

        [BsonElement("amount")]
        [JsonProperty("amount")]
        public decimal Amount { get; set; } //if isMinimumCumulativeAmount
    }
    public class TopSpenderSpendBy
    {

        [BsonElement("topSpenderType")]
        [JsonProperty("topSpenderType")]
        public string TopSpenderType { get; set; }

        [BsonElement("spendOccurence")]
        [JsonProperty("spendOccurence")]
        public SpendOccurence SpendOccurence { get; set; }
    }
    public class CumulativeBy
    {
        [BsonElement("cumulativeType")]
        [JsonProperty("cumulativeType")]
        public string CumulativeType { get; set; }

        [BsonElement("amount")]
        [JsonProperty("amount")]
        public AmountCumulative Amount { get; set; }

        [BsonElement("count")]
        [JsonProperty("count")]
        public CountCumulative Count { get; set; }
    }
    public class Cumulative
    {
        [BsonElement("cumulativeDuration")]
        [JsonProperty("cumulativeDuration")]
        public string CumulativeDuration { get; set; }

        [BsonElement("recurrenceType")]
        [JsonProperty("recurrenceType")]
        public string RecurrenceType { get; set; } // 1. daily, 2. weekly, 3. monthly

        [BsonElement("cumulativeBy")]
        [JsonProperty("cumulativeBy")]
        public CumulativeBy CumulativeBy { get; set; }
    }
    public class AmountCumulative
    {


        [BsonElement("spendBy")]
        [JsonProperty("spendBy")]
        public SpendBy SpendBy { get; set; }
    }
    public class CountCumulative
    {

        [BsonElement("count")]
        [JsonProperty("count")]
        public int Count { get; set; }

        [BsonElement("mode")]
        [JsonProperty("mode")]
        public string Mode { get; set; }

        [BsonElement("isMinimumAmount")]
        [JsonProperty("isMinimumAmount")]
        public bool IsMinimumAmount { get; set; }

        [BsonElement("amount")]
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
    }
    #endregion



    #endregion


    #region Option

    public class RewardOptionType
    {
        [BsonElement("isMultipleCurrency")]
        [JsonProperty("isMultipleCurrency")]
        public int IsMultipleCurrency { get; set; }

        [BsonElement("rewardType")]
        [JsonProperty("rewardType")]
        public string RewardType { get; set; } //point || cashback || voucher

        [BsonIgnoreIfNull]
        [BsonElement("points")]
        [JsonProperty("points")]
        public RewardTypePoints Points { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("cashback")]
        [JsonProperty("cashback")]
        public RewardTypeCashback Cashback { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("voucherDetails")]
        [JsonProperty("voucherDetails")]
        public RewardTypeVoucher VoucherDetails { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("promoVoucherDetails")]
        [JsonProperty("promoVoucherDetails")]
        public RewardTypePromoVoucher PromoVoucherDetails { get; set; }


        [BsonIgnoreIfNull]
        [BsonElement("rewardExpiryOnDemand")]
        [JsonProperty("rewardExpiryOnDemand")]
        public bool RewardExpiryOnDemand { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("subscriptionDetails")]
        [JsonProperty("subscriptionDetails")]
        public RewardTypeSubscription SubscriptionDetails { get; set; }
        [BsonIgnoreIfNull]
        [BsonElement("bundleDetails")]
        [JsonProperty("bundleDetails")]
        public RewardTypeBundle BundleDetails { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("multiCurrencyRandomizedDetails")]
        [JsonProperty("MultiCurrencyRandomizedDetails")]
        public MultiCurrencyRandomized MultiCurrencyRandomizedDetails { get; set; }

        // Slab Based
        [BsonIgnoreIfNull]
        [BsonElement("rowNumber")]
        [JsonProperty("rowNumber")]
        public int RowNumber { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("amount")]
        [JsonProperty("amount")]
        public AmountType Amount { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("count")]
        [JsonProperty("count")]
        public CountType Count { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class AmountType
    {
        [BsonElement("amountConditionType")]
        [JsonProperty("amountConditionType")]
        public string AmountConditionType { get; set; }

        [BsonElement("maxAmount")]
        [JsonProperty("maxAmount")]
        public int MaxAmount { get; set; }

        [BsonElement("minAmount")]
        [JsonProperty("minAmount")]
        public int MinAmount { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class CountType
    {
        [BsonElement("transactionCount")]
        [JsonProperty("transactionCount")]
        public int TransactionCount { get; set; }

        [BsonElement("transactionCountType")]
        [JsonProperty("transactionCountType")]
        public int TransactionCountType { get; set; }

        [BsonElement("range")]
        [JsonProperty("range")]
        public RangeType Range { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class RangeType
    {
        [BsonElement("fromTransactionCount")]
        [JsonProperty("fromTransactionCount")]
        public int FromTransactionCount { get; set; }

        [BsonElement("uptoTransactionCount")]
        [JsonProperty("uptoTransactionCount")]
        public int UptoTransactionCount { get; set; }
    }

    public class MultiCurrencyRandomized
    {
        [BsonElement("budget")]
        [JsonProperty("budget")]
        public decimal Budget { get; set; }

        [BsonElement("templateId")]
        [JsonProperty("templateId")]
        public string TemplateId { get; set; }

        [BsonElement("pointCurrency")]
        [JsonProperty("pointCurrency")]
        public MultiCurrencyRewardTypeRandomized PointCurrency { get; set; }

        [BsonElement("cashbackCurrency")]
        [JsonProperty("cashbackCurrency")]
        public MultiCurrencyRewardTypeRandomized CashbackCurrency { get; set; }

        [BsonElement("promoVoucherCurrency")]
        [JsonProperty("promoVoucherCurrency")]
        public PromoVoucherCurrency PromoVoucherCurrency { get; set; }

        [BsonElement("maxTxnPerCustomer")]
        [JsonProperty("maxTxnPerCustomer")]
        public int MaxTxnPerCustomer { get; set; }
    }

    public class MultiCurrencyRewardTypeRandomized : RewardValueTypeRandomized
    {
        [BsonElement("budgetPercentage")]
        [JsonProperty("budgetPercentage")]
        public decimal BudgetPercentage { get; set; }

        [BsonElement("expirePolicy")]
        [JsonProperty("expirePolicy")]
        public string ExpirePolicy { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("durationType")]
        [JsonProperty("durationType")]
        public string DurationType { get; set; } = "Days";

        [BsonIgnoreIfNull]
        [BsonElement("durationValue")]
        [JsonProperty("durationValue")]
        public int DurationValue { get; set; } = 0;
    }

    public class PromoVoucherCurrency : RewardTypePromoVoucher
    {
        [BsonElement("budgetPercentage")]
        [JsonProperty("budgetPercentage")]
        public decimal BudgetPercentage { get; set; }

        [BsonElement("perceivedValue")]
        [JsonProperty("perceivedValue")]
        public decimal PerceivedValue { get; set; }

        //[BsonElement("promoVoucherType")]
        //[JsonProperty("promoVoucherType")]
        //public string PromoVoucherType { get; set; }
    }

    public class RandomizedDetails
    {
        [BsonElement("topEarnerAmount")]
        [JsonProperty("topEarnerAmount")]
        public decimal TopEarnerAmount { get; set; }

        [BsonElement("budgetPercentage")]
        [JsonProperty("budgetPercentage")]
        public decimal BudgetPercentage { get; set; }

        [BsonElement("bottomEarnerAmount")]
        [JsonProperty("bottomEarnerAmount")]
        public decimal BottomEarnerAmount { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class RewardTypeSubscription
    {
        [BsonElement("subscriptionId")]
        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }

        [BsonElement("subscriptionName")]
        [JsonProperty("subscriptionName")]
        public string SubscriptionName { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("isAutoActivation")]
        [JsonProperty("isAutoActivation")]
        public bool IsAutoActivation { get; set; } = false;
    }

    [BsonIgnoreExtraElements]
    public class RewardTypeBundle
    {
        [BsonElement("bundleId")]
        [JsonProperty("bundleId")]
        public string BundleId { get; set; }
    }

    public class RewardTypePromoVoucher : DealsVoucher
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

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [BsonElement("staticValidDate")]
        [JsonProperty("staticValidDate")]
        public DateTime StaticValidDate { get; set; }

        [BsonElement("dynamicValidDay")]
        [JsonProperty("dynamicValidDay")]
        public int DynamicValidDay { get; set; } = 0;

        [BsonElement("quantity")]
        [JsonProperty("quantity")]
        public int Quantity { get; set; } = 0;
    }

    [BsonIgnoreExtraElements]
    public class RewardTypePoints : BudgetControl
    {
        [BsonElement("pointType")]
        [JsonProperty("pointType")]
        public string PointType { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("pointsFixed")]
        [JsonProperty("pointsFixed")]
        public RewardValueTypeFixed PointsFixed { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("pointsPercentage")]
        [JsonProperty("pointsPercentage")]
        public RewardValueTypePercentage PointsPercentage { get; set; }

        [BsonElement("expirePolicy")]
        [JsonProperty("expirePolicy")]
        public string ExpirePolicy { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("durationType")]
        [JsonProperty("durationType")]
        public string DurationType { get; set; } = "Days";

        [BsonIgnoreIfNull]
        [BsonElement("durationValue")]
        [JsonProperty("durationValue")]
        public int DurationValue { get; set; } = 0;

    }

    [BsonIgnoreExtraElements]
    public class RewardTypeCashback : BudgetControl
    {
        [BsonElement("cashBackType")]
        [JsonProperty("cashBackType")]
        public string CashBackType { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("cashBackFixed")]
        [JsonProperty("cashBackFixed")]
        public RewardValueTypeFixed CashBackFixed { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("cashBackPercentage")]
        [JsonProperty("cashBackPercentage")]
        public RewardValueTypePercentage CashBackPercentage { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("cashBackRandomized")]
        [JsonProperty("cashBackRandomized")]
        public RewardValueTypeRandomized CashBackRandomized { get; set; }
    }
    public class RewardTypeVoucher
    {

        [BsonElement("lob")]
        [JsonProperty("lob")]
        public string LOB { get; set; }

        [BsonElement("catalogueCode")]
        [JsonProperty("catalogueCode")]
        public List<string> CatalogueCode { get; set; }

        [BsonElement("categoryCode")]
        [JsonProperty("categoryCode")]
        public string CategoryCode { get; set; }

        [BsonElement("brandCode")]
        [JsonProperty("brandCode")]
        public string BrandCode { get; set; }

        [BsonElement("denomination")]
        [JsonProperty("denomination")]
        public string Denomination { get; set; }

        [BsonElement("validity")]
        [JsonProperty("validity")]
        public string Validity { get; set; }

        [BsonElement("quantity")]
        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [BsonElement("sku")]
        [JsonProperty("sku")]
        public string SKU { get; set; }

        [BsonElement("voucherCost")]
        [JsonProperty("voucherCost")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal VoucherCost { get; set; }

        [BsonElement("isCheckedOTTMerchant")]
        [JsonProperty("isCheckedOTTMerchant")]
        public string IsCheckedOTTMerchant { get; set; } // OTT / Merchant
    }

    public class RewardValueTypeFixed
    {
        [BsonElement("value")]
        [JsonProperty("value")]
        public decimal Value { get; set; }
    }
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
    public class RewardValueTypeRandomized
    {
        [BsonElement("budget")]
        [JsonProperty("budget")]
        public decimal Budget { get; set; }

        [BsonElement("median")]
        [JsonProperty("median")]
        public decimal Median { get; set; }

        [BsonElement("templateId")]
        [JsonProperty("templateId")]
        public string TemplateId { get; set; }

        [BsonElement("topEarnerAmount")]
        [JsonProperty("topEarnerAmount")]
        public decimal TopEarnerAmount { get; set; }

        [BsonElement("topTransactionPercentage")]
        [JsonProperty("topTransactionPercentage")]
        public decimal TopTransactionPercentage { get; set; }

        [BsonElement("bottomEarnerAmount")]
        [JsonProperty("bottomEarnerAmount")]
        public decimal BottomEarnerAmount { get; set; }

        [BsonElement("bottomTransactionPercentage")]
        [JsonProperty("bottomTransactionPercentage")]
        public decimal BottomTransactionPercentage { get; set; }

        [BsonElement("dailyUnusedBudget")]
        [JsonProperty("dailyUnusedBudget")]
        public string DailyUnusedBudget { get; set; }//lapse || add to pool

        [BsonElement("maxTxnPerCustomer")]
        [JsonProperty("maxTxnPerCustomer")]
        public int MaxTxnPerCustomer { get; set; }

        [BsonElement("assuredCashbak")]
        [JsonProperty("assuredCashbak")]
        public int AssuredCashbak { get; set; }

        [BsonElement("isMinAssuredPoints")]
        [JsonProperty("isMinAssuredPoints")]
        public bool IsMinAssuredPoints { get; set; }

    }

    [BsonIgnoreExtraElements]
    public class BudgetControl
    {
        [BsonElement("isBudgetControl")]
        [JsonProperty("isBudgetControl")]
        public bool IsBudgetControl { get; set; } = false;

        [BsonElement("budgetValue")]
        [JsonProperty("budgetValue")]
        public decimal BudgetValue { get; set; } = 0;
    }
    #endregion


    #region Content
    [BsonIgnoreExtraElements]
    public class Content
    {
        [BsonElement("campaignDescription")]
        [JsonProperty("campaignDescription")]
        public string CampaignDescription { get; set; }

        [BsonElement("rewardNarration")]
        [JsonProperty("rewardNarration")]
        public string RewardNarration { get; set; }
        [BsonIgnoreIfNull]
        [BsonElement("referalrewardNarration")]
        [JsonProperty("referalrewardNarration")]
        public string ReferalrewardNarration { get; set; }
        [BsonIgnoreIfNull]
        [BsonElement("referalUnlockedrewardNarration")]
        [JsonProperty("referalUnlockedrewardNarration")]
        public string ReferalUnlockedrewardNarration { get; set; }


        [BsonElement("unlockCondition")]
        [JsonProperty("unlockCondition")]
        public string UnlockCondition { get; set; }


        [BsonElement("unlockTermAndCondition")]
        [JsonProperty("unlockTermAndCondition")]
        public string UnlockTermAndCondition { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("referrerUnlockTermAndCondition")]
        [JsonProperty("referrerUnlockTermAndCondition")]
        public string ReferrerUnlockTermAndCondition { get; set; }



        [BsonIgnoreIfNull]
        [BsonElement("cTAUrl")]
        [JsonProperty("cTAUrl")]
        public string CTAUrl { get; set; }
        [BsonIgnoreIfNull]
        [BsonElement("cTAReferrerUrl")]
        [JsonProperty("cTAReferrerUrl")]
        public string? CTAReferrerUrl { get; set; }

        [BsonElement("containerName")]
        [JsonProperty("containerName")]
        public string ContainerName { get; set; }


        [BsonElement("folderName")]
        [JsonProperty("folderName")]
        public string FolderName { get; set; }

        [BsonElement("termAndCondition")]
        [JsonProperty("termAndCondition")]
        public string TermAndCondition { get; set; }

        [BsonElement("images")]
        [JsonProperty("images")]
        public List<ECImage> Images { get; set; }

        [BsonElement("imageCount")]
        [JsonProperty("imageCount")]
        public int ImageCount { get; set; }

        [BsonElement("onDemandExpiryNarration")]
        [JsonProperty("onDemandExpiryNarration")]
        public string OnDemandExpiryNarration { get; set; }

        [BsonElement("genericLockCard")]
        [JsonProperty("genericLockCard")]
        public LockCard GenericLockCard { get; set; }

        [BsonElement("referralLockCard")]
        [JsonProperty("referralLockCard")]
        public LockCard ReferralLockCard { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class LockCard
    {
        ////Generic
        [BsonElement("tncTitle")]
        [JsonProperty("tncTitle")]
        public string TncTitle { get; set; }

        [BsonElement("tncSubTitle")]
        [JsonProperty("tncSubTitle")]
        public string tncSubTitle { get; set; }

        [BsonElement("headerBottomDrawer")]
        [JsonProperty("headerBottomDrawer")]
        public string HeaderBottomDrawer { get; set; }

        [BsonElement("tncDescription")]
        [JsonProperty("tncDescription")]
        public string TncDescription { get; set; }

        [BsonElement("cTALabel1")]
        [JsonProperty("cTALabel1")]
        public string CTALabel1 { get; set; }

        [BsonElement("cTALabel2")]
        [JsonProperty("cTALabel2")]
        public string CTALabel2 { get; set; }

        [BsonElement("cTAURL2")]
        [JsonProperty("cTAURL2")]
        public string CTAURL2 { get; set; }

    }
    public class ECImage
    {
        [BsonElement("blobLocationUrl")]
        [JsonProperty("blobLocationUrl")]
        public string BlobLocationUrl { get; set; }

        [BsonElement("size")]
        [JsonProperty("size")]
        public string Size { get; set; }

        [BsonElement("actualFileName")]
        [JsonProperty("actualFileName")]
        public string ActualFileName { get; set; }

        [BsonElement("blioImageDetails")]
        [JsonProperty("blioImageDetails")]
        public string BlioImageDetails { get; set; }

        [BsonElement("uID")]
        [JsonProperty("uID")]
        public string UID { get; set; }

    }
    #endregion


    #region Alert
    public class Alert
    {
        [BsonElement("templateName")]
        [JsonProperty("templateName")]
        public string TemplateName { get; set; }

        [BsonElement("budget")]
        [JsonProperty("budget")]
        public Budget Budget { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("responsysTemplateId")]
        [JsonProperty("responsysTemplateId")]
        public string ResponsysTemplateId { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("unlockResponsysTemplateId")]
        [JsonProperty("unlockResponsysTemplateId")]
        public string UnlockResponsysTemplateId { get; set; }

        [BsonElement("reverseStamping")]
        [JsonProperty("reverseStamping")]
        public bool ReverseStamping { get; set; }
    }
    #endregion

    #region MarchentVPASegment
    [BsonIgnoreExtraElements]
    public class VpaSegmentOnOff
    {
        [BsonElement("vpaSegmentOffline")]
        [JsonProperty("vpaSegmentOffline")]
        public string VpaSegmentOffline { get; set; }

        [BsonElement("lstVpaSegmentOffline")]
        [JsonProperty("lstVpaSegmentOffline")]
        public List<string> LstVpaSegmentOffline { get; set; }

        [BsonElement("vpaSegmentOnline")]
        [JsonProperty("vpaSegmentOnline")]
        public string VpaSegmentOnline { get; set; }

        [BsonElement("lstVpaSegmentOnline")]
        [JsonProperty("lstVpaSegmentOnline")]
        public List<string> LstVpaSegmentOnline { get; set; }
    }
    #endregion

    #region MembershipReward
    [BsonIgnoreExtraElements]
    public class MembershipReward
    {
        [BsonElement("isMembershipReward")]
        [JsonProperty("isMembershipReward")]
        public bool IsMembershipReward { get; set; }

        [BsonElement("subscriptionBasedRewards")]
        [JsonProperty("subscriptionBasedRewards")]
        public List<SubscriptionBasedReward> SubscriptionBasedRewards { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class SubscriptionBasedReward
    {
        [BsonElement("subscriptionId")]
        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }

        [BsonElement("membershipType")]
        [JsonProperty("membershipType")]
        public string MembershipType { get; set; }

        [BsonElement("rewardOption")]
        [JsonProperty("rewardOption")]
        public List<RewardOptionType> RewardOption { get; set; }
    }

    #endregion

    public class Budget
    {
        [BsonElement("range")]
        [JsonProperty("range")]
        public List<BudgetData> Range { get; set; }
    }

    public class BudgetData
    {
        [BsonElement("from")]
        [JsonProperty("from")]
        public int From { get; set; }

        [BsonElement("to")]
        [JsonProperty("to")]
        public int To { get; set; }

        [BsonElement("userID")]
        [JsonProperty("userID")]
        public string UserID { get; set; }

        [BsonElement("template")]
        [JsonProperty("template")]
        public Template Template { get; set; }
    }

    public class Template
    {
        [BsonElement("subject")]
        [JsonProperty("subject")]
        public string Subject { get; set; }

        [BsonElement("body")]
        [JsonProperty("body")]
        public string Body { get; set; }
    }
    #endregion

    public class DealsVoucher
    {
        [BsonElement("brandCode")]
        [JsonProperty("brandCode")]
        public string BrandCode { get; set; }

        [BsonElement("categoryCode")]
        [JsonProperty("categoryCode")]
        public string CategoryCode { get; set; }

        [BsonElement("startDate")]
        [JsonProperty("startDate")]
        public DateTime? StartDate { get; set; }

        [BsonElement("skuName")]
        [JsonProperty("skuName")]
        public string SkuName { get; set; }
    }
}
