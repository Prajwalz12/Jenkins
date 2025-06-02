using Azure.Core;
using Domain.Models.CampaignModel;
using Domain.Models.Common.TransactionModel;
using Domain.Models.CustomerModel;
using Domain.Models.TransactionModel;
using Domain.Processors;
using Domain.Services;
using EventManagerWorker.Models;
using EventManagerWorker.Services.MongoServices.OncePerDestination;
using EventManagerWorker.Utility;
using EventManagerWorker.Utility.Enum;
using Extensions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Utility;
using EarnCampaign = Domain.Models.CampaignModel.EarnCampaign;
using PaymentDirect = Domain.Models.CampaignModel.PaymentDirect;
using Transaction = Domain.Models.TransactionModel.Transaction;
using TransactionDetail = Domain.Models.Common.TransactionModel.TransactionDetail;

namespace EventManagerWorker.Processors
{
    public class DirectCampaignService
    {
        private readonly ILogger<DirectCampaignService> _logger;
        private readonly ICustomerEventService _customerEventService;

        private readonly WebUIDatabaseService _webUIDatabaseService;
        private readonly ProcessorService _processorService;
        private readonly IMerchantMaster _merchantMasterService;
        private readonly IOncePerDestionVpaId _oncePerDestionVpaService;

        public DirectCampaignService(
                ILogger<DirectCampaignService> logger,
            ICustomerEventService customerEventService,
            WebUIDatabaseService webUIDatabaseService,
            ProcessorService processorService,
            IMerchantMaster merchantMasterService,
            IOncePerDestionVpaId oncePerDestionVpaService
        )
        {
            _logger = logger;
            _customerEventService = customerEventService;
            _webUIDatabaseService = webUIDatabaseService;
            _processorService = processorService;
            _merchantMasterService = merchantMasterService;
            _oncePerDestionVpaService = oncePerDestionVpaService;
        }

        public Domain.Models.TransactionModel.ProcessedTransaction ProcessNonCumulativeCampaigns(Domain.Models.TransactionModel.ProcessedTransaction transaction, List<EarnCampaign> nonCumulativeCampaigns, string preFix)
        {
            _logger.LogInformation($"=======================Direct campaign ProcessNonCumulativeCampaigns Start {preFix} ===============================");
            IEnumerable<MatchedCampaign> matchedCampaigns = new List<MatchedCampaign>();
            double? transactionAmount = transaction.TransactionRequest.TransactionDetail.Amount;
            double? loanAmount = transaction.TransactionRequest.TransactionDetail.LoanAmount;
            var _txnReqDetail = transaction.TransactionRequest.TransactionDetail;
            var _mobileNumber = transaction.TransactionRequest.TransactionDetail.Customer.MobileNumber;
            var _txnEventId = transaction.TransactionRequest.EventId;
            var childEventCode = transaction.TransactionRequest.ChildEventCode;
            var transactionDateTime = transaction.TransactionRequest.TransactionDetail.DateTime;
            var _txnEvntTyp = transaction.TransactionRequest.EventId.GetEventCodeByEventName();
            string referrarMobile = transaction.TransactionRequest.TransactionDetail?.ReferAndEarn?.Referrer;
            bool isReferandEarn = (bool)transaction.TransactionRequest.TransactionDetail?.ReferAndEarn?.IsReferAndEarn;
            string Email = transaction.Customer?.Email;

            const StringComparison _strOrd = StringComparison.OrdinalIgnoreCase;


            MatchedCampaign _setCamTyp(EarnCampaign campaign, bool isDirect, bool isLock, bool isUnlock, bool isReferralReward, int transactionCount)
            {
                _logger.LogInformation($"{preFix} Campaign from nonCumulativeCampaigns ::{JsonConvert.SerializeObject(campaign)}");
                bool isReferral = campaign.Filter.IsRefferalProgram;
                return new MatchedCampaign
                {
                    IsDirect = isDirect,
                    LOB = campaign.LOB,
                    IsLock = isLock,
                    IsUnLock = isUnlock,
                    CampaignId = campaign.Id,
                    EventType = _txnEvntTyp,
                    ChildEventCode = childEventCode,
                    OfferType = campaign.OfferType,
                    RewardCriteria = campaign.RewardCriteria,
                    RewardOptions = campaign.Filter.IsMembershipReward ? _processorService.GetMembershipRewardOptions(campaign) : _processorService.GetRewardOptions(campaign),
                    StartDate = campaign.StartDate,
                    EndDate = campaign.EndDate,
                    Narration = isLock ? campaign.Content.UnlockCondition : campaign.Content.RewardNarration,
                    IsOncePerCampaign = campaign.OncePerCampaign,
                    OnceInLifeTime = campaign.OnceInLifeTime,
                    CTAUrl = campaign.Content.CTAUrl, //chetan
                    UnlockTermAndCondition = campaign.Content.UnlockTermAndCondition,
                    IsAssuredCashback = campaign.Filter.IsAssuredCashback,
                    IsReferralProgram = campaign.Filter.IsRefferalProgram,
                    ReferralRewardOptions = isReferral == true ? _processorService.GetReferralRewardOptions(campaign) : null,
                    ReferalrewardNarration = isReferral == true ? (isLock ? campaign.Content?.ReferalUnlockedrewardNarration : campaign.Content.ReferalrewardNarration) : string.Empty,
                    ReferalUnlockTermAndCondition = isReferral == true ? campaign.Content.ReferrerUnlockTermAndCondition : string.Empty,
                    CTAReferrerUrl = isReferral == true ? campaign.Content.CTAReferrerUrl : string.Empty,
                    IsReferralReward = isReferralReward,
                    CampaignStatus = campaign.Status,
                    OnDemandExpiryNarration = campaign.Content.OnDemandExpiryNarration,
                    IsAssuredPoints = campaign.Filter.IsAssuredPoints,
                    ResponsysTemplateId = campaign.Alert.ResponsysTemplateId,
                    UnlockResponsysTemplateId = campaign.Alert.UnlockResponsysTemplateId,
                    CompanyCode = campaign.CompanyCode,
                    IsAssuredMultipleCurrency = campaign.Filter.IsAssuredMultiCurrency,
                    ExcludeFromBudgetCapping = campaign.ExcludeFromBudgetCapping,
                    NoOfQualifiedTransactionsCount = transactionCount,
                    SlabType = campaign.SlabType,
                    IsSlabBasedRewarding = campaign.IsSlabBasedRewarding,
					ReverseStamping = campaign.Alert.ReverseStamping,
                    Email = Email,
                    BflCampaignId = campaign?.BFLCampaignId
                };
            }
            //_logger.LogInformation("{PreFix} Matched Campaigns Before Loop Start : {matchedCampaigns}  count:{count}", preFix, JsonConvert.SerializeObject(matchedCampaigns), matchedCampaigns.Count());
            _logger.LogInformation("{PreFix} Matched Campaigns Before Loop Start : {matchedCampaigns}  count:{count}", preFix, string.Join(",", matchedCampaigns.Select(c => c.CampaignId)), matchedCampaigns.Count());
            foreach (var campaign in nonCumulativeCampaigns)
            {
                var transactionCount = 0;
                //_logger.LogInformation($"Direct Campaign from nonCumulativeCampaigns ::{JsonConvert.SerializeObject(campaign)}");
                _logger.LogInformation($"{preFix} Direct Campaign from nonCumulativeCampaigns ::{JsonConvert.SerializeObject(campaign.Id)}");
                //_logger.LogInformation($"Direct Campaign txn referrer mob::{JsonConvert.SerializeObject(transaction.TransactionRequest.TransactionDetail.ReferAndEarn?.Referrer)}");
                // Spend event
                var campaignEndDateTime = campaign.EndDate;
                var isReferral = campaign.Filter.IsRefferalProgram;

                var direct = campaign?.RewardCriteria?.Direct ?? null;
                var _offerType = campaign?.OfferType; // Generic, Triple Reward
                var _rewardIssuance = campaign?.RewardCriteria?.RewardIssuance; //Direct, Withlockandunlock
                var _directEventType = campaign?.RewardCriteria?.OfferEventDirect; //Activity,Payment,Lending
                //var _lockEventType = campaign?.RewardCriteria?.OfferEventLock; //Activity,Payment,Lending
                //var _unlockEventType = campaign?.RewardCriteria?.OfferEventUnlock; //Activity,Payment,Lending

                var oncePerDestinationVPA = _directEventType == EventTypeEnum.PAYMENT ? (direct.Event as PaymentDirect).OncePerDestinationVPA : false;
                bool isReferralReward = true;

                var directEvent = campaign.RewardCriteria?.Direct;
                var _directEventName = directEvent.EventName;

                _logger.LogInformation("{PreFix} :: Campaign TransactionType => Direct,{isReferral}", preFix, isReferral);

                if (campaign.Filter.IsRefferalProgram && string.IsNullOrEmpty(transaction.TransactionRequest.TransactionDetail.ReferAndEarn?.Referrer))
                {
                    _logger.LogInformation($"{preFix} campaign.Filter.IsRefferalProgram:{campaign.Filter.IsRefferalProgram},referralMob:{transaction.TransactionRequest.TransactionDetail.ReferAndEarn?.Referrer}");
                    continue;
                }
                if (isReferral)
                {
                    if (string.IsNullOrEmpty(referrarMobile) || !isReferandEarn)
                    {
                        _logger.LogInformation("{PreFix} :: Campaign is referral direct, referrar mobile{referrarMobile} =>,{isReferral}", preFix, referrarMobile, isReferral);
                        continue;
                    }

                    var referralTransactions = _processorService.GetReferralTransactions(transaction.TransactionRequest?.TransactionDetail?.ReferAndEarn?.Referrer, campaign, _txnEventId);
                    _logger.LogInformation($"{preFix} if (!(referralTransactions.Count <= campaign.RewardCriteria.RefferalProgram.Capping)) txnCount:{referralTransactions.Count},capping:{campaign.RewardCriteria.RefferalProgram.Capping}");

                    if (!(referralTransactions.Count < campaign.RewardCriteria.RefferalProgram.Capping))
                    {
                        isReferralReward = false;
                        //continue;
                    }
                }

                _logger.LogInformation("{PreFix} :: Campaign OfferType => {offerType}", preFix, _offerType);
                _logger.LogInformation($"{preFix} :: Txn Event Id => {_txnEventId}");

                if (string.Equals(_offerType, OfferTypeEnum.GENERAL_OFFERS, _strOrd))
                {
                    if (string.Equals(_rewardIssuance, RewardIssuanceEnum.DIRECT, _strOrd))
                    {
                        var isProductQualified = ProductQualificationHelper.Qualify(transaction.TransactionRequest.Products, campaign?.RewardCriteria?.Direct?.Products);

                        if (!isProductQualified)
                        {
                            _logger.LogInformation("{PreFix} :: Product qualified => Direct,{isReferral}", preFix, isProductQualified);
                            continue;
                        }

                        if (string.Equals(_txnEventId, EventEnum.SPEND, _strOrd))
                        {
                            var paymentDirect = direct?.Event?.GetPaymentDirect();
                            var flag = true;
                            var isSingle = paymentDirect?.TransactionType == "single";
                            //check for single and referral
                            if (isSingle && isReferral)
                            {
                                if (string.IsNullOrEmpty(referrarMobile) || !isReferandEarn)
                                {
                                    _logger.LogInformation("{PreFix} :: Campaign is refral, referrar mobile{referrarMobile} => {isSingle},{isReferral}", preFix, referrarMobile, isSingle, isReferral);
                                    continue;
                                }
                                _logger.LogInformation("{PreFix} :: Campaign TransactionType => {isSingle},{isReferral}", preFix, isSingle, isReferral);
                                var referralTransactions = _processorService.GetReferralTransactions(transaction.TransactionRequest?.TransactionDetail?.ReferAndEarn?.Referrer, campaign, _txnEventId);
                                if (!(referralTransactions.Count < campaign.RewardCriteria.RefferalProgram.Capping))
                                {
                                    isReferralReward = false;
                                    //continue;
                                }
                            }
                            _logger.LogInformation("{PreFix} :: Campaign TransactionType => {isSingle}", preFix, isSingle);
                            if (isSingle)
                            {
                                var single = paymentDirect?.Single;
                                var eMandate = paymentDirect?.Emandate;

                                _logger.LogInformation("{PreFix} :: Campaign Single => => {single}", preFix, JsonConvert.SerializeObject(single));

                                if (single != null)
                                {
                                    if (single.IsTransactionAmount)
                                    {
                                        if (!(transactionAmount != null && (double)transactionAmount >= (double)single.MinTransactionAmount))
                                        {
                                            flag = false;
                                            continue;
                                        }
                                    }
                                    //bfl-2059 emandate check changed by KaranMehra 
                                    if (transaction.TransactionRequest.TransactionDetail.Emandate == false)
                                    {
                                        if (paymentDirect?.Emandate == true)
                                        {
                                            _logger.LogInformation($"{preFix} : {campaign.Id} not qualified as emandate is true");
                                            continue;
                                        }
                                    }
                                    if ((single.isTransactionCount && single.TransactionCount > 0) || campaign.IsSlabBasedRewarding)
                                    {
                                        var configuredPaymentCategories = paymentDirect?.PaymentCategories;
                                        // Filter By Payment Installation
                                        var configuredPaymentInstruments = paymentDirect?.PaymentInstruments;

                                        var configuredMerchant = paymentDirect?.Merchant;

                                        var isCurrentTransactionPassPaymentCategoryCheck = configuredPaymentCategories.Contains(transaction.TransactionRequest.TransactionDetail.Type);
                                        _logger.LogInformation($" {preFix} Direct => Single => IsCurrentTransactionPassPaymentCategoryCheck : {isCurrentTransactionPassPaymentCategoryCheck}");
                                        if (!isCurrentTransactionPassPaymentCategoryCheck)
                                        {
                                            flag = false;
                                            continue;
                                        }
                                        var isCurrentTransactionPassPaymentInstrumentCheck = configuredPaymentInstruments.Intersect(transaction.TransactionRequest.TransactionDetail.Payments.Select(o => o.PaymentInstrument)).Any();

                                        _logger.LogInformation($" {preFix} Direct => Single => IsCurrentTransactionPassPaymentInstrumentCheck : {isCurrentTransactionPassPaymentInstrumentCheck}");
                                        if (!isCurrentTransactionPassPaymentInstrumentCheck)
                                        {
                                            flag = false;
                                            continue;
                                        }
                                        var customerTransactions = new List<Transaction>();

                                        if ((campaign.SubscriptionTypes.Contains("Paid") || campaign.SubscriptionTypes.Contains("Trial")) && campaign.SubscriptionTypes.Contains("regular"))
                                        {
                                            customerTransactions = _processorService.GetTransactions(transaction.Customer.MobileNumber, campaign);
                                            _logger.LogInformation($" {preFix} Total customerTransactions count after calling GetTransactions() with basic filter: {customerTransactions.Count} with  campaign.BFLCampaignId :{campaign.BFLCampaignId}");

                                        }
                                        else if (campaign.SubscriptionTypes.Contains("Paid") && campaign.SubscriptionTypes.Contains("Trial") && !campaign.SubscriptionTypes.Contains("regular"))
                                        {
                                            customerTransactions = _processorService.GetTransactions(transaction.Customer.MobileNumber, campaign);
                                            _logger.LogInformation($" {preFix} Total customerTransactions count after calling GetTransactions() with basic filter: {customerTransactions.Count} with  campaign.BFLCampaignId :{campaign.BFLCampaignId}");
                                        }

                                        else if (campaign.SubscriptionTypes.Contains("Paid") || campaign.SubscriptionTypes.Contains("Trial"))
                                        {
                                            customerTransactions = _processorService.GetTransactionsAfterPrimeActivation(transaction.Customer.MobileNumber, campaign, transaction.Customer.PrimeActivationDate);
                                            _logger.LogInformation($" {preFix} Total customerTransactions count after calling GetTransactionsAfterPrimeActivation() with basic filter: {customerTransactions.Count} with  campaign.BFLCampaignId :{campaign.BFLCampaignId}");

                                        }
                                        else
                                        {
                                            // Get Transaction Of Customer 
                                            customerTransactions = _processorService.GetTransactions(transaction.Customer.MobileNumber, campaign);
                                            _logger.LogInformation($" {preFix} Total customerTransactions count after calling GetTransactions() with basic filter: {customerTransactions.Count} with  campaign.BFLCampaignId :{campaign.BFLCampaignId}");

                                            // Filter By Payment Category
                                        }
                                        _logger.LogInformation($"{preFix} Total customerTransactions count after basic filter: {customerTransactions.Count} with  campaign.BFLCampaignId :{campaign.BFLCampaignId}");

                                        var filteredCustomerTransactions = customerTransactions
                                            .Where(o => configuredPaymentCategories.Contains(o.TransactionDetail.Type))
                                            .Where(o => (o.TransactionDetail.Payments.Select(p => p.PaymentInstrument)).Intersect(configuredPaymentInstruments).Any())
                                            .Where(o => (!single.IsTransactionAmount || o.TransactionDetail.Amount >= (double)single.MinTransactionAmount))
                                            .Where(o => (o.TransactionDetail.DateTime <= transactionDateTime));

                                        _logger.LogInformation($" {preFix} Total filteredCustomerTransactions count after scategory,paymentinstruments and amount  filter: {filteredCustomerTransactions.ToList().Count} with  campaign.BFLCampaignId :{campaign.BFLCampaignId}");
                                        transactionCount = filteredCustomerTransactions.Count();
                                        var totalTransactionCount = 0;

                                        var transctionPaymentCategory = transaction.TransactionRequest.TransactionDetail.Type;
                                        var currentTransactionMerchantDealer = transaction.TransactionRequest.TransactionDetail.MerchantOrDealer;
                                        var currentTransactionBiller = transaction.TransactionRequest.TransactionDetail.Biller;

                                        var currentCampaignMerchantDealer = paymentDirect?.Merchant;
                                        var currentCampaignBiller = paymentDirect?.BBPS;
                                        if (flag && configuredPaymentCategories.Contains("subPurchase") && transctionPaymentCategory == "subPurchase")
                                        {
                                            if (!paymentDirect.SubscriptionId.Contains(transaction.TransactionRequest.TransactionDetail.SubscriptionId))
                                            {
                                                _logger.LogInformation($"{preFix} No matching campaign for subscription id in transaction request.");
                                                continue;
                                            }
                                        }
                                        _logger.LogInformation($"{preFix} p2moff/on --->>>>transctionPaymentCategory:{transctionPaymentCategory}");
                                        if (IsP2P(configuredPaymentCategories, transctionPaymentCategory) || IsP2MOFF(configuredPaymentCategories, transctionPaymentCategory) || IsP2MON(configuredPaymentCategories, transctionPaymentCategory))
                                        {
                                            // check for unique rewarding p2p
                                            if (oncePerDestinationVPA == true)
                                            {
                                                var totalTxnCountForOncePerPayee = _oncePerDestionVpaService.GetTransactionCountForOncePerPayee(transaction.Customer.MobileNumber, campaign.BFLCampaignId);
                                                _logger.LogInformation($"Total Txn Count for Once Per Payee: {totalTxnCountForOncePerPayee}");

                                                totalTransactionCount = Convert.ToInt32(totalTxnCountForOncePerPayee + 1);

                                                var destinationVpaId = transaction?.TransactionRequest?.TransactionDetail?.Customer?.DestinationVPAId;
                                                if (!string.IsNullOrEmpty(destinationVpaId))
                                                {
                                                    var request = new Models.MobileCampaignOncePerDestinationRequest()
                                                    {
                                                        CampaignId = campaign.BFLCampaignId,
                                                        MobileNumber = transaction.Customer.MobileNumber,
                                                        CustomerId = transaction.Customer.MobileNumber,
                                                        MerchantId = null,
                                                        BillerId = null,
                                                        DestinationVPA = destinationVpaId,
                                                    };
                                                    var result = _oncePerDestionVpaService.GetMobileCampaignDestination(request);

                                                    _logger.LogInformation($" {preFix} MobileCampaignOncePerDestination_GetMobileCampaignDestinationAsync result {JsonConvert.SerializeObject(result)}");

                                                    if (!string.IsNullOrEmpty(result?.Id))
                                                    {
                                                        _logger.LogInformation($"{preFix} No unique rewarding for this combination of DestinationVPAId for P2P. Already rewarded for this.");
                                                        _logger.LogInformation($"{preFix} after IsP2P  transactionCount in mapping Table for OncePerPayee:: {totalTransactionCount}");
                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    _logger.LogInformation($"No unique rewarding for this DestinationVPAId for P2P. DestinationVPAId id is empty .");
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                _logger.LogInformation($" {preFix} before IsP2P:totalTransactionCount::{totalTransactionCount}");
                                                _logger.LogInformation($" {preFix} before IsP2P:filteredCustomerTransactions::{JsonConvert.SerializeObject(totalTransactionCount)}");
                                             
                                                totalTransactionCount += filteredCustomerTransactions.Count(o => (o.TransactionDetail.Type == "p2p") || (o.TransactionDetail.Type == "P2MOF") || (o.TransactionDetail.Type == "P2MON"));
                                                if (totalTransactionCount > 0 && (transaction.TransactionRequest.TransactionDetail.Type == "P2MON" || transaction.TransactionRequest.TransactionDetail.Type == "P2MOF"))
                                                {
                                                   
                                                    var vpaTransactionList = filteredCustomerTransactions.Where(o => (o.TransactionDetail.Type == "P2MOF") || (o.TransactionDetail.Type == "P2MON")).ToList();
                                                    _logger.LogInformation($" {preFix} vpaTransactionList:filteredCustomerTransactions::{JsonConvert.SerializeObject(vpaTransactionList)}");

                                                    foreach (var vpaTransaction in vpaTransactionList)
                                                    {
                                                        var isValidVpaTransactionForCampaign = false;
                                                        if (string.Equals(vpaTransaction.TransactionDetail.Type, "P2MOF", _strOrd) || string.Equals(vpaTransaction.TransactionDetail.Type, "p2mof", _strOrd))
                                                        {
                                                            isValidVpaTransactionForCampaign = _processorService.ApplyVPASegmentFilter(campaign, vpaTransaction.TransactionDetail, "P2MOF", "Payment", preFix, "Direct");

                                                        }
                                                        else if (string.Equals(vpaTransaction.TransactionDetail.Type, "P2MON", _strOrd) || string.Equals(vpaTransaction.TransactionDetail.Type, "p2mon", _strOrd))
                                                        { 
                                                            isValidVpaTransactionForCampaign = _processorService.ApplyVPASegmentFilter(campaign, vpaTransaction.TransactionDetail, "P2MON", "Payment", preFix, "Direct");
                                                        }

                                                        if (!isValidVpaTransactionForCampaign)
                                                        {
                                                            totalTransactionCount -= 1;
                                                        }
                                                    }
                                                }
                                                //totalTransactionCount += filteredCustomerTransactions.Count(o => (o.TransactionDetail.Type == "p2p" || o.TransactionDetail.Type == "P2MOF" || o.TransactionDetail.Type == "P2MON") && o.TransactionDetail.DateTime <= transactionDateTime);
                                                _logger.LogInformation($" {preFix} after IsP2P:totalTransactionCount::{totalTransactionCount}");
                                            }
                                        }
                                        else if (configuredPaymentCategories.Contains("p2m") && transctionPaymentCategory == "p2m")
                                        {
                                            _logger.LogInformation($" {preFix}before foreach ValidateMerchantSegment:totalTransactionCount::{totalTransactionCount}");
                                            var p2mTypeCustomerTransactions = filteredCustomerTransactions.Where((o => o.TransactionDetail.Type == "p2m"));
                                            _logger.LogInformation($"p2mTypeCustomerTransactions Count: {p2mTypeCustomerTransactions.Count()}");
                                            foreach (var p2mTypeCutomerTransaction in p2mTypeCustomerTransactions)
                                            {

                                                //if (configuredMerchant.MerchantSegment == "Any")
                                                //{
                                                //    totalTransactionCount += 1;
                                                //}
                                                //else
                                                //{
                                                // unique p2p check BFL-2099
                                                if (oncePerDestinationVPA != false)
                                                {
                                                    var totalTxnCountForOncePerPayee = _oncePerDestionVpaService.GetTransactionCountForOncePerPayee(transaction.Customer.MobileNumber, campaign.BFLCampaignId);
                                                    _logger.LogInformation($"Total Txn Count for Once Per Payee: {totalTxnCountForOncePerPayee}");

                                                    totalTransactionCount = Convert.ToInt32(totalTxnCountForOncePerPayee + 1);
                                                    var merchantId = transaction?.TransactionRequest?.TransactionDetail?.MerchantOrDealer?.Id;

                                                    if (!string.IsNullOrEmpty(merchantId))
                                                    {
                                                        var request = new Models.MobileCampaignOncePerDestinationRequest()
                                                        {
                                                            CampaignId = campaign.BFLCampaignId,
                                                            MobileNumber = transaction.Customer.MobileNumber,
                                                            CustomerId = transaction.Customer.MobileNumber,
                                                            MerchantId = transaction?.TransactionRequest?.TransactionDetail?.MerchantOrDealer?.Id,
                                                            BillerId = null,
                                                            DestinationVPA = null,
                                                        };
                                                        var result = _oncePerDestionVpaService.GetMobileCampaignDestination(request);
                                                        _logger.LogInformation($" {preFix} MobileCampaignOncePerDestination_GetMobileCampaignDestinationAsync result {JsonConvert.SerializeObject(result)}");

                                                        if (!string.IsNullOrEmpty(result?.Id))
                                                        {
                                                            _logger.LogInformation($"No unique rewarding for this combination of MerchantId for P2M. Already rewarded for this.");
                                                            continue;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        _logger.LogInformation($"No unique rewarding MerchantId is empty .");
                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    var responseFlag = _processorService.ValidateMerchantSegment(currentCampaignMerchantDealer, p2mTypeCutomerTransaction.TransactionDetail);
                                                    if (responseFlag)
                                                    {
                                                        _logger.LogInformation($" {preFix} before p2mTypeCustomerTransactions: totalTransactionCount::{totalTransactionCount}");
                                                        totalTransactionCount += 1;
                                                        _logger.LogInformation($" {preFix} after p2mTypeCustomerTransactions:totalTransactionCount::{totalTransactionCount}");
                                                    }
                                                }
                                            }
                                        }
                                        else if (configuredPaymentCategories.Contains("bbps") && transctionPaymentCategory == "bbps")
                                        {
                                            var bbpsTypeCutomerTransactions = filteredCustomerTransactions.Where(o => o.TransactionDetail.Type == "bbps");
                                            _logger.LogInformation($" {preFix} currentCampaignBiller.AnyCategory: {currentCampaignBiller.AnyCategory}");
                                            // unique p2p check BFL-2099
                                            if (oncePerDestinationVPA == true)
                                            {
                                                var totalTxnCountForOncePerPayee = _oncePerDestionVpaService.GetTransactionCountForOncePerPayee(transaction.Customer.MobileNumber, campaign.BFLCampaignId);
                                                _logger.LogInformation($"Total Txn Count for Once Per Payee: {totalTxnCountForOncePerPayee}");

                                                totalTransactionCount = Convert.ToInt32(totalTxnCountForOncePerPayee + 1);
                                                var billerId = transaction?.TransactionRequest?.TransactionDetail?.Biller?.Id;

                                                if (!string.IsNullOrEmpty(billerId))
                                                {
                                                    var request = new Models.MobileCampaignOncePerDestinationRequest()
                                                    {
                                                        CampaignId = campaign.BFLCampaignId,
                                                        MobileNumber = transaction.Customer.MobileNumber,
                                                        CustomerId = transaction.Customer.MobileNumber,
                                                        MerchantId = null,
                                                        BillerId = billerId,
                                                        DestinationVPA = null,
                                                    };
                                                    var result = _oncePerDestionVpaService.GetMobileCampaignDestination(request);
                                                    _logger.LogInformation($" {preFix} MobileCampaignOncePerDestination_GetMobileCampaignDestinationAsync result {JsonConvert.SerializeObject(result)}");

                                                    if (!string.IsNullOrEmpty(result?.Id))
                                                    {
                                                        _logger.LogInformation($"No unique rewarding for this combination of BillerId for BBPS. Already rewarded for this.");
                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    _logger.LogInformation($"No unique rewarding BillerId for BBPS is empty.");
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                if (currentCampaignBiller.AnyCategory)
                                                {
                                                    _logger.LogInformation($" {preFix} bbpsTypeCutomerTransactions.Count(): {bbpsTypeCutomerTransactions.Count()}");
                                                    totalTransactionCount += bbpsTypeCutomerTransactions.Count();
                                                    //totalTransactionCount += filteredCustomerTransactions.Count(o => (o.TransactionDetail.Type == "p2p" || o.TransactionDetail.Type == "P2MOF" || o.TransactionDetail.Type == "P2MON" || o.TransactionDetail.Type == "bbps") && o.TransactionDetail.DateTime <= transactionDateTime);

                                                }

                                                else
                                                {
                                                    foreach (var bbpsTypeCutomerTransaction in bbpsTypeCutomerTransactions)
                                                    {

                                                        var bbpsTypeCutomerTransactionBiller = bbpsTypeCutomerTransaction.TransactionDetail.Biller;
                                                        if (_processorService.IsBillerValid(currentCampaignBiller, bbpsTypeCutomerTransactionBiller))
                                                        {
                                                            totalTransactionCount += 1;
                                                        }
                                                    }
                                                    _logger.LogInformation($" {preFix} not any.Count(): {totalTransactionCount}");
                                                }
                                            }
                                        }
                                        else if (configuredPaymentCategories.Contains("Insurance") && transctionPaymentCategory == "Insurance")
                                        {
                                            _logger.LogInformation($" {preFix} before Insurance:totalTransactionCount::{totalTransactionCount}");
                                            totalTransactionCount += filteredCustomerTransactions.Count(o => (o.TransactionDetail.Type == "Insurance"));
                                            //totalTransactionCount += filteredCustomerTransactions.Count(o => o.TransactionDetail.Type == "Insurance" && o.TransactionDetail.DateTime <= transactionDateTime);
                                            _logger.LogInformation($" {preFix} after Insurance:totalTransactionCount::{totalTransactionCount}");

                                        }
                                        else if (configuredPaymentCategories.Contains("Confirm") && transctionPaymentCategory == "Confirm")
                                        {
                                            _logger.LogInformation($" {preFix} before Confirm:totalTransactionCount::{totalTransactionCount}");
                                            totalTransactionCount += filteredCustomerTransactions.Count(o => (o.TransactionDetail.Type == "Confirm"));
                                            //totalTransactionCount += filteredCustomerTransactions.Count(o => o.TransactionDetail.Type == "Confirm" && o.TransactionDetail.DateTime <= transactionDateTime);

                                            _logger.LogInformation($" {preFix} after Confirm:totalTransactionCount::{totalTransactionCount}");

                                        }
                                        else if (configuredPaymentCategories.Contains("Partpay") && transctionPaymentCategory == "Partpay")
                                        {
                                            _logger.LogInformation($" {preFix} before Partpay:totalTransactionCount::{totalTransactionCount}");
                                            totalTransactionCount += filteredCustomerTransactions.Count(o => (o.TransactionDetail.Type == "Partpay"));
                                            //totalTransactionCount += filteredCustomerTransactions.Count(o => o.TransactionDetail.Type == "Partpay" && o.TransactionDetail.DateTime <= transactionDateTime);

                                            _logger.LogInformation($" {preFix} after Partpay:totalTransactionCount::{totalTransactionCount}");

                                        }
                                        else if (configuredPaymentCategories.Contains("Drawdown") && transctionPaymentCategory == "Drawdown")
                                        {
                                            _logger.LogInformation($" {preFix} before Drawdown:totalTransactionCount::{totalTransactionCount}");
                                            totalTransactionCount += filteredCustomerTransactions.Count(o => (o.TransactionDetail.Type == "Drawdown"));
                                            //totalTransactionCount += filteredCustomerTransactions.Count(o => o.TransactionDetail.Type == "Drawdown" && o.TransactionDetail.DateTime <= transactionDateTime);
                                            _logger.LogInformation($" {preFix} after Drawdown:totalTransactionCount::{totalTransactionCount}");

                                        }
                                        _logger.LogInformation($" {preFix} totalTransactionCount: {totalTransactionCount} == single.TransactionCount: {single.TransactionCount}");
                                        if (single.isTransactionCount || campaign.IsSlabBasedRewarding)
                                        {
                                            // for testing 
                                            _logger.LogInformation($"Value of isTransactionCount : {single.isTransactionCount}, TransactionCountType: {single.TransactionCountType}, totalTransactionCount : {totalTransactionCount}, single.TransactionCount: {single.TransactionCount}");
                                            if (single.TransactionCountType == 3 && !(totalTransactionCount == single.TransactionCount))
                                            {
                                                _logger.LogInformation($"skipping campaign not qualified:TransactionCountType is{single.TransactionCountType}(equal)");
                                                flag = false;
                                                continue;
                                            }
                                            if (single.TransactionCountType == 2 && !(totalTransactionCount > single.TransactionCount))
                                            {
                                                _logger.LogInformation($"skipping campaign not qualified :TransactionCountType is{single.TransactionCountType}(greater than)");
                                                flag = false;
                                                continue;
                                            }
                                            if (single.TransactionCountType == 1 && !(totalTransactionCount < single.TransactionCount))
                                            {
                                                _logger.LogInformation($"skipping campaign not qualified:TransactionCountType is{single.TransactionCountType}(less than)");
                                                flag = false;
                                                continue;
                                            }
                                        }
                                    }
                                }
                            }

                            // check if its direct
                            if (flag && string.Equals(_rewardIssuance, RewardIssuanceEnum.DIRECT, _strOrd))
                            {
                                // add as direct and continue with other
                                matchedCampaigns = matchedCampaigns.Append(_setCamTyp(campaign, true, false, false, isReferralReward,  transactionCount));
                            }
                            continue;
                        }
                        else if (string.Equals(_txnEventId, EventEnum.ANY, _strOrd))
                        {
                            // this case will be tested in 2nd phase.
                            matchedCampaigns = matchedCampaigns.Append(_setCamTyp(campaign, true, false, false, isReferralReward,  transactionCount));
                            continue;
                        }
                        else if (string.Equals(_txnEventId, EventEnum.GENERIC_ACTIVITY, _strOrd))
                        {
                            var _childEventCode = directEvent?.Event?.GetGenericActivity()?.ChildEventCode;
                            if (string.Equals(childEventCode, _childEventCode, _strOrd))
                            {
                                // this case will be tested in 2nd phase.
                                matchedCampaigns = matchedCampaigns.Append(_setCamTyp(campaign, true, false, false, isReferralReward,  transactionCount));
                            }
                            continue;
                        }
                        else if (string.Equals(_txnEventId, EventEnum.WALLET_LOAD, _strOrd))
                        {
                            // check if the Wallet load condition matches
                            // i.e., loadcount, loadinterval, paymentinstrument,
                            // minloadamount.
                            // add as direct and continue with other
                            var _cmpWlCnfg = directEvent?.Event?.GetWalletLoad();
                            bool _flgWltMtch = _processorService.WalletLoadCheck(transaction, campaign, _cmpWlCnfg, "WalletLoad");
                            if (_flgWltMtch)
                            {
                                matchedCampaigns = matchedCampaigns.Append(_setCamTyp(campaign, true, false, false, isReferralReward,  transactionCount));
                                //continue;
                            }
                            continue;
                        }
                        else if (string.Equals(_txnEventId, EventEnum.CLEAR_BOUNCE_EMI, _strOrd))
                        {
                            // if txn emiflag and campaign flag both are false
                            // or if both are true and both amount is equal
                            var _txnEmiPaid = transaction.TransactionRequest.TransactionDetail.EMI.BounceFlg;
                            var _tnxEmiAmt = transaction.TransactionRequest.TransactionDetail.EMI.Amount;
                            var directClearBounceEMI = directEvent?.Event?.GetClearBounceEMI();
                            var _cmpEmiPaid = directClearBounceEMI?.BounceEMIPaid ?? false;
                            var _cmpEmiAmt = directClearBounceEMI.EMIAmount;
                            if ((!_txnEmiPaid && !_cmpEmiPaid) ||
                                (_txnEmiPaid && _cmpEmiPaid && _tnxEmiAmt >= Convert.ToDouble(_cmpEmiAmt)))
                            {
                                matchedCampaigns = matchedCampaigns.Append(_setCamTyp(campaign, true, false, false, isReferralReward, transactionCount));
                                //continue;
                            }
                            continue;
                        }
                        else if (string.Equals(_txnEventId, EventEnum.KYC_COMPLETION, _strOrd))
                        {
                            // if txn kyc is min match to kyc_upgrade
                            // if txn kyc is full match to kyc_update
                            var _kycType = directEvent?.Event?.GetKYC()?.CompletionType;
                            var _txnKycType = transaction.TransactionRequest.TransactionDetail.Customer.KYCUpgradeFlg;
                            var customerKYCCompletionTag = transaction.Customer.KYC.CompletionTag;
                            if ((string.Equals(customerKYCCompletionTag, "Upgrad", _strOrd) && (string.Equals(_txnKycType, "Full", _strOrd) && string.Equals(_kycType, "kyc_upgrad", _strOrd))) || (string.Equals(customerKYCCompletionTag, "Update", _strOrd) && (string.Equals(_txnKycType, "Full", _strOrd) && string.Equals(_kycType, "kyc_update", _strOrd))))
                            {

                                matchedCampaigns = matchedCampaigns.Append(_setCamTyp(campaign, true, false, false, isReferralReward, transactionCount));
                                //continue;
                            }
                            continue;
                        }
                        else if (string.Equals(_txnEventId, EventEnum.EMANDATE_CREATION, _strOrd))
                        {
                            var isQualified = _processorService.ApplyVPASegmentFilter(campaign, transaction.TransactionRequest.TransactionDetail, "P2MON", "Activity", preFix, "Direct");
                            if (isQualified)
                            {
                                matchedCampaigns = matchedCampaigns.Append(_setCamTyp(campaign, true, false, false, isReferralReward, transactionCount));
                            }
                            continue;
                        }
                        else if (string.Equals(_txnEventId, EventEnum.UPI_LITE, _strOrd))
                        {
                            // check if the UPI Lite condition matches
                            // i.e., loadcount, paymentinstrument,
                            // minamount.
                            // add as direct and continue with other
                            var _cmpUPILiteCnfg = directEvent?.Event?.GetUPILite();
                            bool _flgUPILiteMtch = _processorService.UPILiteCheck(transaction, campaign, _cmpUPILiteCnfg, directEvent?.EventName);
                            if (_flgUPILiteMtch)
                            {
                                matchedCampaigns = matchedCampaigns.Append(_setCamTyp(campaign, true, false, false, isReferralReward, transactionCount));
                                //continue;
                            }
                            continue;
                        }
                        else if (string.Equals(_txnEventId, EventEnum.DO_ISSUE, _strOrd))
                        {
                            var directDOISSUE = direct?.Event?.GetDOISSUE();
                            var _d_isLoanApplicable = directDOISSUE?.IsLoanAmountApplicable ?? false;
                            if (_d_isLoanApplicable)
                            {
                                if (loanAmount >= (double)directDOISSUE?.LoanAmount)
                                    matchedCampaigns = matchedCampaigns.Append(_setCamTyp(campaign, true, false, false, isReferralReward, transactionCount));
                            }
                            else
                            {
                                matchedCampaigns = matchedCampaigns.Append(_setCamTyp(campaign, true, false, false, isReferralReward, transactionCount));
                            }
                            continue;
                        }
                        else if (string.Equals(_txnEventId, EventEnum.DI_COMPLETED, _strOrd))
                        {
                            var directDICOMPLETED = direct?.Event?.GetDICOMPLETED();
                            var _d_isLoanApplicable = directDICOMPLETED?.IsLoanAmountApplicable ?? false;
                            if (_d_isLoanApplicable)
                            {
                                if (loanAmount >= (double)directDICOMPLETED?.LoanAmount)
                                    matchedCampaigns = matchedCampaigns.Append(_setCamTyp(campaign, true, false, false, isReferralReward, transactionCount));
                            }
                            else
                            {
                                matchedCampaigns = matchedCampaigns.Append(_setCamTyp(campaign, true, false, false, isReferralReward, transactionCount));
                            }
                            continue;
                        }
                        else
                        {
                            // for all other cases
                            matchedCampaigns = matchedCampaigns.Append(_setCamTyp(campaign, true, false, false, isReferralReward, transactionCount));
                            continue;
                        }
                    }
                }
                else if (string.Equals(_offerType, OfferTypeEnum.TRIPLE_REWARD, _strOrd))
                {
                    // Nothing to do
                }
            }
            _logger.LogInformation("{PreFix} Matched Campaigns After Loop End : {matchedCampaigns}", preFix, JsonConvert.SerializeObject(matchedCampaigns));
            transaction.MatchedCampaigns = matchedCampaigns.ToList();
            _logger.LogInformation("=================================== ProcessNonCumulativeCampaigns End ===============================");
            return transaction;

            static bool IsP2P(List<string> configuredPaymentCategories, string transctionPaymentCategory)
            {
                return configuredPaymentCategories.Contains("p2p") && transctionPaymentCategory == "p2p";
            }
            static bool IsP2MOFF(List<string> configuredPaymentCategories, string transctionPaymentCategory)
            {
                return configuredPaymentCategories.Contains("P2MOF") && transctionPaymentCategory == "P2MOF";
            }
            static bool IsP2MON(List<string> configuredPaymentCategories, string transctionPaymentCategory)
            {
                return configuredPaymentCategories.Contains("P2MON") && transctionPaymentCategory == "P2MON";
            }

            bool IsPaymentCatAndPaymentInstrumentMatch(PaymentDirect paymentHybdlkunlkObj, TransactionDetail _txnDetail, string preFix)
            {

                var _txnPymtCat = _txnDetail.Type;
                var _txnPymtInstrmt = _txnDetail.Payments.Select(o => o.PaymentInstrument).ToList();
                _logger.LogInformation($"{preFix} paymentHybdlkunlkObj => WithLockUnlock => WithLockUnlockData : {JsonConvert.SerializeObject(paymentHybdlkunlkObj)}");

                // if campaign is of any type, then add in the qualified campaign and continue
                if (string.Equals(paymentHybdlkunlkObj.TransactionType, "Any", _strOrd))
                {
                    // add this campaign in cumulative campaign and continue 
                    return true;
                }

                var _pymtCats = paymentHybdlkunlkObj.PaymentCategories;
                _logger.LogInformation($"{preFix} paymentHybdlkunlkObj => WithLockUnlock => CampaignPaymentCategories : {JsonConvert.SerializeObject(_pymtCats)}");
                _logger.LogInformation($" {preFix} paymentHybdlkunlkObj => WithLockUnlock => TransactionPaymentCategories : {_txnPymtCat}");
                if (!_pymtCats.Contains(_txnPymtCat, StringComparer.OrdinalIgnoreCase))
                {
                    _logger.LogInformation($" {preFix} paymentHybdlkunlkObj=> WithLockUnlock => Categories Not Matched.");
                    // don't qualify campaign if payment category doesn't match
                    return false;
                }

                // here we check if any of the transaction paymentinstrument exists in campaign paymentinstrument
                var _pymtInstrmt = paymentHybdlkunlkObj.PaymentInstruments;
                bool _txnPymtInstrmtHasMatch = _pymtInstrmt.Select(s => s).Intersect(_txnPymtInstrmt, StringComparer.OrdinalIgnoreCase).Any();

                _logger.LogInformation($" {preFix} paymentHybdlkunlkObj => WithLockUnlock => CampaignPaymentInstruments : {JsonConvert.SerializeObject(_pymtInstrmt)}");
                _logger.LogInformation($" {preFix} paymentHybdlkunlkObj => WithLockUnlock => TransactionPaymentInstruments : {JsonConvert.SerializeObject(_txnPymtInstrmt)} :: _txnPymtInstrmtHasMatch:{_txnPymtInstrmtHasMatch}");

                if (!_txnPymtInstrmtHasMatch) { _logger.LogInformation($"{preFix} paymentHybdlkunlkObj => WithLockUnlock => Payment Instrument dosenot matched : {JsonConvert.SerializeObject(_pymtInstrmt)}"); return false; }

                return true;

            }

            bool ValidateIsCampaignP2P(PaymentDirect paymentHybdlkunlkObj, TransactionDetail _txnDetail, string prefix)
            {
                var _txnPymtCat = _txnDetail.Type;
                var _txnamt = _txnDetail.Amount;
                var _mintxnamt = paymentHybdlkunlkObj.Single?.MinTransactionAmount;

                var _isMintxnamt = (bool)paymentHybdlkunlkObj.Single?.IsTransactionAmount;
                if (_isMintxnamt)
                {
                    if (!(_txnamt >= Convert.ToDouble(_mintxnamt)))
                        return false;
                }
                var _txnPymtInstrmt = _txnDetail.Payments.Select(o => o.PaymentInstrument);
                if (string.Equals(_txnPymtCat, "p2p", _strOrd) || _txnPymtCat == "P2MOF" || _txnPymtCat == "P2MON")
                {
                    if (string.Equals(paymentHybdlkunlkObj.TransactionType, "Any", _strOrd))
                    {
                        // add this campaign in cumulative campaign and continue 
                        return true;
                    }
                    else if (string.Equals(_txnPymtCat, "p2p", _strOrd))
                    {
                        // add this campaign in cumulative campaign and continue 
                        _logger.LogInformation("returned as true p2p");
                        return true;
                    }
                    else if (string.Equals(_txnPymtCat, "P2MOF", _strOrd))
                    {
                        // add this campaign in cumulative campaign and continue 
                        return true;
                    }
                    else if (string.Equals(_txnPymtCat, "P2MON", _strOrd))
                    {
                        // add this campaign in cumulative campaign and continue 
                        return true;
                    }
                }
                return false;
            }

            bool ValidateIsCampaignFlexi(PaymentDirect paymentHybdlkunlkObj, TransactionDetail _txnDetail, string prefix)
            {
                var _txnPymtCat = _txnDetail.Type;
                var _txnamt = _txnDetail.Amount;
                var _mintxnamt = paymentHybdlkunlkObj.Single?.MinTransactionAmount;

                var _isMintxnamt = (bool)paymentHybdlkunlkObj.Single?.IsTransactionAmount;
                if (_isMintxnamt)
                {
                    if (!(_txnamt >= Convert.ToDouble(_mintxnamt)))
                        return false;
                }
                var _txnPymtInstrmt = _txnDetail.Payments.Select(o => o.PaymentInstrument);
                if (string.Equals(_txnPymtCat, "Insurance", _strOrd) || _txnPymtCat == "Confirm" || _txnPymtCat == "Partpay" || _txnPymtCat == "Drawdown")
                {
                    if (string.Equals(paymentHybdlkunlkObj.TransactionType, "Any", _strOrd))
                    {
                        // add this campaign in cumulative campaign and continue 
                        return true;
                    }
                    else if (string.Equals(_txnPymtCat, "Insurance", _strOrd))
                    {
                        // add this campaign in cumulative campaign and continue 
                        _logger.LogInformation("returned as true Insurance");
                        return true;
                    }
                    else if (string.Equals(_txnPymtCat, "Confirm", _strOrd))
                    {
                        // add this campaign in cumulative campaign and continue 
                        return true;
                    }
                    else if (string.Equals(_txnPymtCat, "Partpay", _strOrd))
                    {
                        // add this campaign in cumulative campaign and continue 
                        return true;
                    }
                    else if (string.Equals(_txnPymtCat, "Drawdown", _strOrd))
                    {
                        // add this campaign in cumulative campaign and continue 
                        return true;
                    }
                }
                return false;
            }

            //bbps
            bool IsCampaignContainsBBPS(PaymentDirect paymentHybdlkunlkObj, TransactionDetail _txnDetail, string prefix)
            {
                var _txnPymtCat = _txnDetail.Type;
                var _txnamt = _txnDetail.Amount;
                var _mintxnamt = paymentHybdlkunlkObj.Single?.MinTransactionAmount;

                // Check for BBPS
                if (string.Equals(_txnPymtCat, "bbps", _strOrd))
                {
                    //var _cmpgnBllrCats = _hybdLkkUnlk.BBPS.BillerCategories;
                    var paymentDetail = paymentHybdlkunlkObj;
                    _logger.LogInformation($" {preFix} paymentHybdlkunlkObj => WithLockUnlock => BBPS => PaymentDetails : {JsonConvert.SerializeObject(paymentDetail)}");
                    bool isBillerCategoryMatched = false;
                    if (paymentDetail.BBPS.AnyCategory)
                    {
                        _logger.LogInformation($" {preFix} paymentHybdlkunlkObj => WithLockUnlock => BBPS => PaymentDetails => AnyCategory : {paymentDetail.BBPS.AnyCategory}");
                        isBillerCategoryMatched = true;
                    }
                    else
                    {
                        var _cmpgnBllrCats = paymentDetail.BBPS.BillerCategories;
                        var _txnBllrCat = _txnDetail.Biller;
                        _logger.LogInformation($" {preFix} paymentHybdlkunlkObj => WithLockUnlock => BBPS => CampaignCategories : {JsonConvert.SerializeObject(_cmpgnBllrCats)}");
                        _logger.LogInformation($" {preFix} paymentHybdlkunlkObj => WithLockUnlock => BBPS => TransactionCategories : {JsonConvert.SerializeObject(_txnBllrCat)}");
                        foreach (var campaignBillerCategory in _cmpgnBllrCats)
                        {
                            _logger.LogInformation($" {preFix} paymentHybdlkunlkObj => WithLockUnlock => BBPS => campaignBillerCategory : {JsonConvert.SerializeObject(campaignBillerCategory)}");
                            if (string.Equals(campaignBillerCategory.BillerCategory, _txnBllrCat.Category, _strOrd))
                            {
                                // Split Biller IDs by comma and trim whitespace
                                var billerIds = campaignBillerCategory.Biller
                                    .Split(',')
                                    .Select(b => b.Trim())
                                    .ToList();

                                // Check if any of the billerIds match the transaction biller ID
                                if (billerIds.Contains("Any", StringComparer.OrdinalIgnoreCase) ||
                                    billerIds.Contains(_txnBllrCat.Id, StringComparer.OrdinalIgnoreCase))
                                {
                                    isBillerCategoryMatched = true;
                                    break;
                                }
                            }
                        }
                    }
                    _logger.LogInformation($" {preFix} paymentHybdlkunlkObj => WithLockUnlock => BBPS => IsBillerCategoryMatched : {isBillerCategoryMatched}");
                    if (isBillerCategoryMatched)
                    {
                        if (string.Equals(paymentDetail.TransactionType, "Cumulative", _strOrd))
                        {
                            _logger.LogInformation($" {preFix} Adding To Cumulative Qualified Campaign.");
                            // add this campaign in cumulative campaign and continue 
                            return false;
                        }
                        _logger.LogInformation($" {preFix} isBillerCategoryMatched.");
                        //quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                        //continue;
                        return true;
                    }
                }
                return false;
            }
            bool IsCampaignContainsMerchant(PaymentDirect paymentHybdlkunlkObj, TransactionDetail _txnDetail, string prefix)
            {

                var _txnPymtCat = _txnDetail.Type;
                var _txnamt = _txnDetail.Amount;
                var _mintxnamt = paymentHybdlkunlkObj.Single?.MinTransactionAmount;


                //P2M
                if (string.Equals(_txnPymtCat, "p2m", _strOrd))
                {

                    var _cmpgnMchntRDlr = paymentHybdlkunlkObj.Merchant;
                    _logger.LogInformation($" {preFix} Campaign MerchantBiller : {JsonConvert.SerializeObject(_cmpgnMchntRDlr)}");
                    // This Code is Done For Any Merchant Check.
                    var merchantType = paymentHybdlkunlkObj.Merchant?.MerchantType;

                    // If Campaign Has Configured For Any Merchant. Then Check MerchantSegment == "Any". If Condition Pass Add into Qualified Campaign List.
                    var rewardMerchants = _webUIDatabaseService.GetMerchantDBEnumValues(preFix, _txnDetail.MerchantOrDealer.Category, _txnDetail.MerchantOrDealer.GroupId, _txnDetail.MerchantOrDealer.Source, _txnDetail.MerchantOrDealer.Id, null, merchantType).GetAwaiter().GetResult();
                    //var rewardMerchants = _merchantMasterService.GetMerchantMasterValues(new Models.MerchantEnumRequest()
                    //{
                    //    Category = _txnDetail.MerchantOrDealer.Category,
                    //    GroupMerchantId = _txnDetail.MerchantOrDealer.GroupId,
                    //    MerchantId = _txnDetail.MerchantOrDealer.Id,
                    //    Source = _txnDetail.MerchantOrDealer.Source,
                    //    TripleReward = null,
                    //    MerchantType = merchantType
                    //});

                    if (rewardMerchants == null)
                    {
                        return false;
                    }
                    if (string.Equals(_cmpgnMchntRDlr.MerchantSegment, "Any", _strOrd))
                    {
                        if (string.Equals(paymentHybdlkunlkObj.TransactionType, "Cumulative", _strOrd))
                        {
                            _logger.LogInformation($" {preFix} Adding To Cumulative Qualified Campaign.");
                            // add this campaign in cumulative campaign and continue 
                            //quaifiedCumulativeCampaigns = quaifiedCumulativeCampaigns.Append(campaign);
                            //continue;
                            return false;
                        }
                        _logger.LogInformation($" {preFix} Adding To Qualified Campaign.");
                        //quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                        //continue;
                        return true;
                    }
                    else
                    {
                        var currentTransactionMerchantDealer = transaction.TransactionRequest.TransactionDetail.MerchantOrDealer;

                        var currentCampaignMerchantDealer = paymentHybdlkunlkObj.Merchant;

                        return _processorService.ValidateMerchantSegment(currentCampaignMerchantDealer, transaction.TransactionRequest.TransactionDetail);

                    }
                }

                return false;
            }

        }

    }
}
