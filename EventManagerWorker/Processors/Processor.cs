using Domain.Builders;
using RewardModel = Domain.Models.RewardModel;
using Domain.Services;
using Domain.Services.Kafka;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utility;
using CampaignModel = Domain.Models.CampaignModel;
using CustomerModel = Domain.Models.CustomerModel;
using TransactionModel = Domain.Models.TransactionModel;
using CommonModel = Domain.Models.Common;
using Extensions;
using Domain.Models.EnumMaster;
using Domain.Models.CampaignModel;
using EventManagerWorker.Processors;
using Domain.Models.TransactionModel;
using Domain.Models.Mappers;
using Domain.Models.Common.CustomerModel;
using EventManagerWorker.Utility.Enum;
using Microsoft.Extensions.Configuration;
using Flag = Domain.Models.Common.CustomerModel.Flag;
using Microsoft.AspNetCore.Mvc;
using EventManagerWorker.Models;
using EarnCampaign = Domain.Models.CampaignModel.EarnCampaign;
using EventManagerWorker.Services.MongoServices.OncePerDestination;
using Azure.Core;
using Serilog.Events;
using UtilService;
using Domain.Models.CustomerModel;
using LobMonthlyBudgetCappingApi;
using Domain.Models.Common.TransactionModel;

namespace Domain.Processors
{
    public class Processor
    {
        //private readonly string TopicName = "Transactions";
        private readonly ILogger<Processor> _logger;
        private readonly ProcessorService _processorService;
        //private readonly QueueServiceHelper _queueServiceHelper;
        private readonly MessageQueueService _messageQueueService;
        private readonly ICustomerEventService _customerEventService;
        private readonly WebUIDatabaseService _webUIDatabaseService;
        private readonly ReferralUnlockCampaignService _referralUnlockCampaign;
        private readonly DirectCampaignService _directCampaignService;
        private readonly UnlockCampaignService _unlockCampaignService;
        private readonly LockCampaignService _lockCampaignService;
        private readonly CustomerService _customerService;
        private readonly CustomerVersionService _customerVersionService;
        private readonly EventManagerApi.EventManagerApiClient _eventManagerApiClient;
        private string transactionMobileNumber = null;
        private string transactionReferenceNumber = null;
        private string referraleNumber = null;
        private string preFix = null;
        private readonly IConfiguration _configuration;
        private readonly string OfferMapTopicName;
        private readonly UtilServiceClient _utilServiceClient;
        private readonly IMerchantMaster _merchantMasterService;
        private readonly IOncePerDestionVpaId _oncePerDestionVpaService;
        private readonly LobMonthlyBudgetCappingApiClient _loanMonthlyBudgetCappingApiClient;
        private readonly MongoService.MongoServiceClient _mongoServiceClient;
        public Processor
            (
            ILogger<Processor> logger,
            ProcessorService processorService,
            MessageQueueService messageQueueService,
            ICustomerEventService customerEventService,
            WebUIDatabaseService webUIDatabaseService,
            ReferralUnlockCampaignService referralUnlockCampaign,
            DirectCampaignService directCampaignService,
            UnlockCampaignService unlockCampaignService,
            LockCampaignService lockCampaignService,
            UtilServiceClient utilServiceClient,
             CustomerService customerService,
             CustomerVersionService customerVersionService,
             EventManagerApi.EventManagerApiClient eventManagerApiClient,
             IConfiguration configuration,
             IMerchantMaster merchantMasterService,
             IOncePerDestionVpaId oncePerDestionVpaService,
             LobMonthlyBudgetCappingApiClient lobMonthlyBudgetCappingApiClient,
             MongoService.MongoServiceClient mongoServiceClient
            )
        {
            _logger = logger;
            _processorService = processorService;
            _messageQueueService = messageQueueService;
            _customerEventService = customerEventService;
            _webUIDatabaseService = webUIDatabaseService;
            _referralUnlockCampaign = referralUnlockCampaign;
            _directCampaignService = directCampaignService;
            _unlockCampaignService = unlockCampaignService;
            _lockCampaignService = lockCampaignService;
            _customerService = customerService;
            transactionMobileNumber = null;
            transactionReferenceNumber = null;
            preFix = null;
            _eventManagerApiClient = eventManagerApiClient;
            _customerVersionService = customerVersionService;
            _configuration = configuration;
            _utilServiceClient = utilServiceClient;
            OfferMapTopicName = _configuration["KafkaSettings:OfferMapTopicName"];
            _merchantMasterService = merchantMasterService;
            _oncePerDestionVpaService = oncePerDestionVpaService;
            _loanMonthlyBudgetCappingApiClient = lobMonthlyBudgetCappingApiClient;
            _mongoServiceClient = mongoServiceClient;
        }

        public async Task Process(List<TransactionModel.ProcessedTransaction> processedTransactions)
        {
            _logger.LogInformation($"Processed Transaction Count {processedTransactions.Count}");

            foreach (var processedTransaction in processedTransactions)
            {
                try
                {
                    transactionMobileNumber = processedTransaction.TransactionRequest.TransactionDetail.Customer.MobileNumber;
                    transactionReferenceNumber = processedTransaction.TransactionRequest.TransactionDetail.RefNumber;
                    preFix = GetLogInformation(transactionMobileNumber, transactionReferenceNumber);

                    _logger.LogInformation("{preFix} ==================== Processing Start ========================", preFix);
                    _logger.LogInformation("{preFix} ProcessedTransaction => {0}", preFix, JsonConvert.SerializeObject(processedTransaction));
                    var customer = CustomerOnboarding(processedTransaction).Result;
                    _logger.LogInformation($"{preFix} CustomerOnboarding response : {customer.Type}");
                    processedTransaction.TransactionRequest.TransactionDetail.Customer.CustomerType = customer.Type;
                    processedTransaction.Customer = customer;
                    processedTransaction.Customer.SubscriptionType = string.IsNullOrEmpty(processedTransaction.Customer.SubscriptionType) ? "Regular" : processedTransaction.Customer.SubscriptionType;
                    _logger.LogInformation($"{preFix} Subscription type for: {processedTransaction.Customer.MobileNumber} is: {processedTransaction.Customer.SubscriptionType}");
                    _logger.LogInformation($"{preFix} requestTransactionType : {processedTransaction.TransactionRequest.TransactionDetail.Customer.CustomerType}");
                    referraleNumber = processedTransaction.TransactionRequest.TransactionDetail?.ReferAndEarn?.Referrer;
                    _logger.LogInformation($"{preFix} start UpdateMissedTransaction  : {preFix},{referraleNumber}");

                    var referAndEarn = processedTransaction.TransactionRequest.TransactionDetail?.ReferAndEarn;
                    if (referAndEarn != null && referAndEarn.IsReferAndEarn)
                    {
                        var referrerCustomer = await _customerService.GetByMobileNumberAsync(referAndEarn.Referrer).ConfigureAwait(false);
                        _logger.LogInformation($"{preFix} ReferrerCustomer response : {customer.Type}");
                        if (referrerCustomer != null)
                        {
                            processedTransaction.ReferrerCustomer = referrerCustomer;
                            processedTransaction.ReferrerCustomer.SubscriptionType = string.IsNullOrEmpty(processedTransaction.ReferrerCustomer.SubscriptionType) ? "Regular" : processedTransaction.ReferrerCustomer.SubscriptionType;
                            _logger.LogInformation($"{preFix} Subscription type for: {processedTransaction.ReferrerCustomer.MobileNumber} is: {processedTransaction.ReferrerCustomer.SubscriptionType}");
                        }
                    }

                    try
                    {
                        _processorService.UpdateMissedTransaction(processedTransaction.TransactionRequest, transactionMobileNumber);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"{preFix} ::{ex.Message}");
                    }

                    if (processedTransaction.TransactionRequest.IsRedeemedTransaction())
                    {
                        _logger.LogInformation($"{preFix} This is a Reedem Transaction");
                        continue;
                    }
                    //Check For LoayaltyFraud
                    if (processedTransaction.Customer.IsLoyaltyFraudConfirmed())
                    {
                        _ = _processorService.InsertIntoLoyaltyFraudConfirmedLogs(processedTransaction);
                        _logger.LogInformation("{PreFix} Customer Loyalty Fraud Confirmed.", preFix);
                        continue;
                    }
                    // Campaign Processing 
                    var campaigns = GetCampaigns(processedTransaction);

                    try
                    {
                        _logger.LogInformation($"{preFix} Campaign count After Basic Filter : {campaigns.Count}");
                    }
                    catch { }
                    //var serializedCampaignBeforeEventTypeFilter = JsonConvert.SerializeObject(campaigns);
                    var serializedCampaignBeforeEventTypeFilter = campaigns.Select(c => c.Id);

                    //_logger.LogInformation($"{preFix} Campaign After Basic Filter : {serializedCampaignBeforeEventTypeFilter}");
                    _logger.LogInformation($"{preFix} Campaign After Basic Filter : {string.Join(",", serializedCampaignBeforeEventTypeFilter)}");

                    if (campaigns?.Any() != true)
                    {
                        _logger.LogInformation($"{preFix} No Campaign Found.");
                        ManageNoCampaignQualified(processedTransaction);
                        continue;
                    }

                    List<MongoService.CustomerMobileCampaignMapping> customerCampaignMappings = null;
                    try
                    {
                        customerCampaignMappings = _processorService.GetCustomerMobileCampaignMappings(new MongoService.CustomerMobileCampaignMappingRequest() { MobileNumber = processedTransaction.Customer.MobileNumber }).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"fetch GetCustomerMobileCampaignMappings{preFix} ::{ex.Message}");
                    }


                    _logger.LogInformation($"{preFix} CustomerCampaignMappings : {JsonConvert.SerializeObject(customerCampaignMappings)}");

                    IEnumerable<CampaignModel.EarnCampaign> oncePerCampaignPassedCampaigns = null;

                    if (customerCampaignMappings == null || !customerCampaignMappings.Any())
                    {
                        _logger.LogInformation($"{preFix} => CustomerCampaignMappings : {JsonConvert.SerializeObject(customerCampaignMappings)}");
                        oncePerCampaignPassedCampaigns = campaigns;
                    }
                    else
                    {
                        oncePerCampaignPassedCampaigns = ApplyOncePerCampaignFilter(campaigns, processedTransaction, customerCampaignMappings);

                        _logger.LogInformation($"{preFix} Campaigns After Once Per Campaign {JsonConvert.SerializeObject(oncePerCampaignPassedCampaigns)}");

                        if (oncePerCampaignPassedCampaigns?.Any() != true)
                        {
                            _logger.LogInformation($" {preFix} Campaigns After Once Per Campaign Not Qualified : No Campaigns Passed OncePerCampaigns Filter.");
                            ManageNoCampaignQualified(processedTransaction);
                            continue;
                        }
                    }
                    var customerSummary = _processorService.GetCustomerSummary(processedTransaction.Customer.MobileNumber);

                    var customerSubscription = _processorService.GetCustomer(processedTransaction.Customer.MobileNumber);

                    _logger.LogInformation($"{preFix} CustomerSummary: {JsonConvert.SerializeObject(customerSummary)}");
                    _logger.LogInformation($"{preFix} GetCustomer: {JsonConvert.SerializeObject(customerSubscription)}");

                    var advancedFilterPassedCampaigns = ApplyAdvancedFilter(oncePerCampaignPassedCampaigns, processedTransaction, customerSummary, customerSubscription);

                    //_logger.LogInformation($"advancedFilterPassedCampaigns: {JsonConvert.SerializeObject(advancedFilterPassedCampaigns)}");
                    _logger.LogInformation($"{preFix} advancedFilterPassedCampaigns non cumulative: {string.Join(",", advancedFilterPassedCampaigns.Item1.Select(c => c.Id))}");
                    _logger.LogInformation($"{preFix} advancedFilterPassedCampaigns cumulative: {string.Join(",", advancedFilterPassedCampaigns.Item2.Select(c => c.Id))}");
                    _logger.LogInformation($"{preFix} advancedFilterPassedCampaigns triple rewards: {string.Join(",", advancedFilterPassedCampaigns.Item3.Select(c => c.Id))}");
                    bool isTripleRewarded = false;
                    if (advancedFilterPassedCampaigns.Item3?.Any() == true)
                    {
                        // Apply TripleReward Merchant Check
                        var campaignsAfterTripleRewardMerchantCheck = ApplyTripleRewardMerchantFilter(processedTransaction, advancedFilterPassedCampaigns.Item3);

                        _logger.LogInformation($"{preFix} Campaign After TripleReward Filter : {JsonConvert.SerializeObject(campaignsAfterTripleRewardMerchantCheck)}");

                        if (campaignsAfterTripleRewardMerchantCheck == null || !campaignsAfterTripleRewardMerchantCheck.Any())
                        {
                            _logger.LogInformation("{PreFix} No Qualified Campaign Found To Push in Triple Reward Kafka Topic.", preFix);
                        }
                        else
                        {
                            if (campaignsAfterTripleRewardMerchantCheck.Count() > 1)
                            {
                                _logger.LogInformation("{PreFix} More Than 1 Campaign Qualified For Triple Reward.", preFix);
                            }
                            else
                            {
                                // TO DO : 
                                // Insert into New Collection TripleRewardTransactionRequest and New TransactionId Will be Send in Kafka. 
                                processedTransaction.MatchedCampaigns = new List<TransactionModel.MatchedCampaign>() { SetCampaign(campaignsAfterTripleRewardMerchantCheck.First()) };
                                try
                                {
                                    _logger.LogInformation($"Transaction Pushed in Kafka for tripleReward : {JsonConvert.SerializeObject(processedTransaction)}");
                                    PublishInKafka(processedTransaction, OfferMapTopicName);
                                    _logger.LogInformation("{PreFix} Triple Reward Transaction Data Pushed In Kafka Queue.", preFix);
                                    isTripleRewarded = true;
                                    //continue;
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "{PreFix} Triple Reward Transaction Data Not Pushed In Kafka Queue.{exMessage}", preFix, ex.Message);
                                    _logger.LogError(ex.ToString(), $"{preFix} Attention ! Error Occured. : {ex.Message}");
                                }
                                processedTransaction.MatchedCampaigns = new List<TransactionModel.MatchedCampaign>();

                                TransactionModel.MatchedCampaign SetCampaign(CampaignModel.EarnCampaign campaign)
                                {
                                    return new TransactionModel.MatchedCampaign
                                    {
                                        IsDirect = false,
                                        IsLock = true,
                                        LOB = campaign.LOB,
                                        IsUnLock = false,
                                        CampaignId = campaign.Id,
                                        EventType = "Spend",
                                        ChildEventCode = null,
                                        OfferType = campaign.OfferType,
                                        RewardCriteria = campaign.RewardCriteria,
                                        RewardOptions = campaign.Filter.IsMembershipReward ? _processorService.GetMembershipRewardOptions(campaign) : _processorService.GetRewardOptions(campaign),
                                        StartDate = campaign.StartDate,
                                        EndDate = campaign.EndDate,
                                        Narration = campaign.Content.RewardNarration,
                                        IsOncePerCampaign = campaign.OncePerCampaign,
                                        OnceInLifeTime = campaign.OnceInLifeTime,
                                        CTAUrl = campaign.Content.CTAUrl,
                                        UnlockTermAndCondition = campaign.Content.UnlockTermAndCondition,
                                        IsAssuredCashback = campaign.Filter.IsAssuredCashback,
                                        IsAssuredPoints = campaign.Filter.IsAssuredPoints,
                                        GenericLockCard = campaign.Content.GenericLockCard,
                                        ReferralLockCard = campaign.Content.ReferralLockCard,
                                        CompanyCode = campaign.CompanyCode,
                                        ResponsysTemplateId = campaign.Alert.ResponsysTemplateId,
                                        UnlockResponsysTemplateId = campaign.Alert.UnlockResponsysTemplateId,
                                        IsAssuredMultipleCurrency = campaign.Filter.IsAssuredMultiCurrency,
                                        ExcludeFromBudgetCapping = campaign.ExcludeFromBudgetCapping,
                                        ReverseStamping = campaign.Alert.ReverseStamping,
                                        Email = customer.Email,
                                        BflCampaignId=campaign.BFLCampaignId
                                    };
                                }
                            }
                        }
                    }

                    var qalifiedCampaigns = advancedFilterPassedCampaigns.Item1?.ToList();


                    if (qalifiedCampaigns?.Any() != true)
                    {
                        _logger.LogInformation("{PreFix} No Campaign Qualified.", preFix);
                        ManageNoCampaignQualified(processedTransaction);
                        continue;
                    }
                    ProcessedTransaction finalQualifiedCampaigns = new ProcessedTransaction();
                    _logger.LogInformation($"count before non cumulatve :{processedTransaction.MatchedCampaigns?.Count}");
                    //if triple reward get rewarded then other non cumulative campaign should also qualify.
                    //if we want to stop it then uncomment the below if condition
                    //if (!isTripleRewarded)
                    //{
                    _logger.LogInformation($"{preFix} count before non cumulatve and Non TripleReward :{qalifiedCampaigns?.Count}");
                    //_logger.LogInformation($"qualified campaign after advance filter : {JsonConvert.SerializeObject(qalifiedCampaigns)}");
                    _logger.LogInformation($"{preFix} qualified campaign after advance filter : {string.Join(",", qalifiedCampaigns.Select(c => c.Id))}");
                    var unlockReferralCampaigns = qalifiedCampaigns.Where(x => x.IsUnlockCampaign(processedTransaction).IsUnlock == true && x.Filter.IsRefferalProgram && x.RewardCriteria.WithLockUnlock?.UnlockEvent?.EventName == processedTransaction.TransactionRequest.EventId).ToList();
                    //_logger.LogInformation($"Unlock Referral Campaign Result: {string.Join(",", unlockReferralCampaigns.Select(c => c.Id))}");
                    var directCampaigns = qalifiedCampaigns.Where(x => x.IsDirectCampaign() == true && x.Status == "ACTIVE" && x.EndDate >= DateTime.Today && x.RewardCriteria.Direct?.EventName == processedTransaction.TransactionRequest.EventId).ToList();
                    //_logger.LogInformation($"Direct Result: {string.Join(",", directCampaigns.Select(c => c.Id))}");
                    directCampaigns = await ApplyLobBudgetCappingFilter(directCampaigns, processedTransaction).ConfigureAwait(false);
                    var unlockCampaigns = qalifiedCampaigns.Where(x => x.IsUnlockCampaign(processedTransaction).IsUnlock == true && x.Filter.IsRefferalProgram == false && x.RewardCriteria.WithLockUnlock?.UnlockEvent?.EventName == processedTransaction.TransactionRequest.EventId).ToList();
                    //_logger.LogInformation($"Unlock Campaign Result: {JsonConvert.SerializeObject(unlockCampaigns)}");
                    //_logger.LogInformation($"Unlock Campaign Result: {string.Join(",", unlockCampaigns.Select(c => c.Id))}");
                    //bsm-1281 updated logic for precedance of subscription rewards campaign
                    var lockCampaigns = GetLockCampaigns(qalifiedCampaigns, processedTransaction);
                    lockCampaigns = await ApplyLobBudgetCappingFilter(lockCampaigns, processedTransaction).ConfigureAwait(false);
                    //var lockCampaigns = qalifiedCampaigns.Where(x => x.IsLockCampaign(processedTransaction) == true && x.Status == "ACTIVE" && x.EndDate >= DateTime.Today).ToList();
                    _logger.LogInformation($"lock Campaign Result: {JsonConvert.SerializeObject(lockCampaigns)}");

                    bool isGroupCampaign = IsGroupedCampaign(processedTransaction);
                    _logger.LogInformation($"{preFix} isGroupCampaign: {isGroupCampaign}");
                    List<MatchedCampaign> groupMatchedCampaigns = new List<MatchedCampaign>();
                    bool isReferAndEarn = processedTransaction.TransactionRequest?.TransactionDetail?.ReferAndEarn?.IsReferAndEarn == null ? false : processedTransaction.TransactionRequest.TransactionDetail.ReferAndEarn.IsReferAndEarn;

                    if (isReferAndEarn)
                    {
                        if (unlockReferralCampaigns != null && unlockReferralCampaigns.Any())
                        {
                            _logger.LogInformation("{PreFix} unlockReferralCampaigns:: {unlockReferralCampaigns}", preFix, unlockReferralCampaigns == null ? null : string.Join(",", unlockReferralCampaigns.Select(c => c.Id)));
                            var tempfinalQualifiedCampaigns = _referralUnlockCampaign.ProcessNonCumulativeCampaigns(processedTransaction, unlockReferralCampaigns, preFix);
                            if (isGroupCampaign)
                            {
                                groupMatchedCampaigns.AddRange(tempfinalQualifiedCampaigns.MatchedCampaigns);
                            }
                            else
                            {
                                finalQualifiedCampaigns = tempfinalQualifiedCampaigns;
                            }
                        }
                    }
                    else
                    {
                        if (unlockReferralCampaigns != null && unlockReferralCampaigns.Any() && !isReferAndEarn)
                        {
                            //_logger.LogInformation(" {PreFix} unlockReferralCampaigns:: {unlockReferralCampaigns}", preFix, unlockReferralCampaigns == null ? null : JsonConvert.SerializeObject(unlockReferralCampaigns));
                            _logger.LogInformation("{PreFix} unlockReferralCampaigns:: {unlockReferralCampaigns}", preFix, unlockReferralCampaigns == null ? null : string.Join(",", unlockReferralCampaigns.Select(c => c.Id)));
                            var tempfinalQualifiedCampaigns = _referralUnlockCampaign.ProcessNonCumulativeCampaigns(processedTransaction, unlockReferralCampaigns, preFix);
                            if (isGroupCampaign)
                            {
                                groupMatchedCampaigns.AddRange(tempfinalQualifiedCampaigns.MatchedCampaigns);
                            }
                            else
                            {
                                finalQualifiedCampaigns = tempfinalQualifiedCampaigns;
                            }
                        }
                    }

                    if (directCampaigns != null && directCampaigns.Any() && (finalQualifiedCampaigns.MatchedCampaigns == null || !finalQualifiedCampaigns.MatchedCampaigns.Any()) && !isGroupCampaign)
                    {
                        //_logger.LogInformation(" {PreFix} directCampaigns:: {directCampaigns}", preFix, directCampaigns == null ? null : JsonConvert.SerializeObject(directCampaigns));
                        _logger.LogInformation("{PreFix} directCampaigns:: {directCampaigns}", preFix, directCampaigns == null ? null : string.Join(",", directCampaigns.Select(c => c.Id)));

                        finalQualifiedCampaigns = _directCampaignService.ProcessNonCumulativeCampaigns(processedTransaction, directCampaigns, preFix);
                    }

                    if (directCampaigns != null && directCampaigns.Any() && isGroupCampaign)
                    {
                        _logger.LogInformation($"{preFix} directCampaigns for group campaign");
                        var tempfinalQualifiedCampaigns = _directCampaignService.ProcessNonCumulativeCampaigns(processedTransaction, directCampaigns, preFix);
                        groupMatchedCampaigns.AddRange(tempfinalQualifiedCampaigns.MatchedCampaigns);
                    }

                    if (unlockCampaigns != null && unlockCampaigns.Any() && (finalQualifiedCampaigns.MatchedCampaigns == null || !finalQualifiedCampaigns.MatchedCampaigns.Any()) && !isGroupCampaign)
                    {
                        //_logger.LogInformation(" {PreFix} unlockCampaigns:: {unlockCampaigns}", preFix, unlockCampaigns == null ? null : JsonConvert.SerializeObject(unlockCampaigns));
                        _logger.LogInformation("{PreFix} unlockCampaigns:: {unlockCampaigns}", preFix, unlockCampaigns == null ? null : string.Join(",", unlockCampaigns.Select(c => c.Id)));

                        var unlock = _unlockCampaignService.ProcessNonCumulativeCampaigns(processedTransaction, unlockCampaigns, preFix);
                        var suspendedCampaigns = unlock.MatchedCampaigns.Where(o => o.CampaignStatus == "SUSPENDED").ToList();
                        //suspendedCampaigns.Where(x=>x.CampaignStatus)
                        foreach (var item in suspendedCampaigns)
                        {
                            var tempRecord = _processorService.GetTempTransactions(processedTransaction.TransactionRequest.MobileNumber, item.CampaignId);
                            if (tempRecord == null || !tempRecord.Any())
                            {
                                //suspendedCampaigns.Remove(item);
                                unlock.MatchedCampaigns.Remove(item);
                            }
                        }
                        finalQualifiedCampaigns = unlock;
                    }
                    if (unlockCampaigns != null && unlockCampaigns.Any() && isGroupCampaign)
                    {
                        _logger.LogInformation($"{preFix} unlockCampaigns for group campaign");
                        var unlock = _unlockCampaignService.ProcessNonCumulativeCampaigns(processedTransaction, unlockCampaigns, preFix);
                        var suspendedCampaigns = unlock.MatchedCampaigns.Where(o => o.CampaignStatus == "SUSPENDED").ToList();
                        foreach (var item in suspendedCampaigns)
                        {
                            var tempRecord = _processorService.GetTempTransactions(processedTransaction.TransactionRequest.MobileNumber, item.CampaignId);
                            if (tempRecord == null || !tempRecord.Any())
                            {
                                unlock.MatchedCampaigns.Remove(item);
                            }
                        }
                        groupMatchedCampaigns.AddRange(unlock.MatchedCampaigns);
                    }

                    if (lockCampaigns != null && lockCampaigns.Any() && (finalQualifiedCampaigns.MatchedCampaigns == null || !finalQualifiedCampaigns.MatchedCampaigns.Any()))
                    {
                        _logger.LogInformation(" {PreFix} lockCampaigns:: {lockCampaigns}", preFix, lockCampaigns == null ? null : JsonConvert.SerializeObject(lockCampaigns));

                        finalQualifiedCampaigns = _lockCampaignService.ProcessNonCumulativeCampaigns(processedTransaction, lockCampaigns, preFix);
                    }
                    if (lockCampaigns != null && lockCampaigns.Any() && isGroupCampaign)
                    {
                        _logger.LogInformation($"{preFix} lockCampaigns for group campaign : : {JsonConvert.SerializeObject(lockCampaigns)}");
                        var tempfinalQualifiedCampaigns = _lockCampaignService.ProcessNonCumulativeCampaigns(processedTransaction, lockCampaigns, preFix);
                        groupMatchedCampaigns.AddRange(tempfinalQualifiedCampaigns.MatchedCampaigns);
                    }

                    //_logger.LogInformation($"Group campaign list  : {JsonConvert.SerializeObject(groupMatchedCampaigns) ?? "null"}");
                    _logger.LogInformation($"{preFix} Group campaign list  : {string.Join(",", groupMatchedCampaigns.Select(c => c.CampaignId)) ?? "null"}");
                    //_logger.LogInformation($" finalQualifiedCampaigns.MatchedCampaigns : {JsonConvert.SerializeObject(finalQualifiedCampaigns.MatchedCampaigns) ?? "null"}");
                    _logger.LogInformation($"{preFix}  finalQualifiedCampaigns.MatchedCampaigns : {string.Join(",", finalQualifiedCampaigns.MatchedCampaigns.Select(c => c.CampaignId)) ?? "null"}");

                    if (processedTransaction.TransactionRequest.EventId != "Spend")
                    {
                        InsertInCustomerEvent(processedTransaction);
                    }
                    if (!isTripleRewarded && !(finalQualifiedCampaigns.MatchedCampaigns != null && finalQualifiedCampaigns.MatchedCampaigns.Any()) && !isGroupCampaign)
                    {
                        InsertInTransactionReward(processedTransaction);
                        _logger.LogInformation($"{preFix} No campaign qualified inserted in transaction reward with 00");
                        continue;
                    }
                    // Push into Mongo too.
                    // _ = _processorService.InsertIntoOfferMap(finalQualifiedCampaigns);
                    //Task.FromResult(_queueServiceHelper.PublishToOfferMapAsync(JsonConvert.SerializeObject(nonCumulativeTransactions))).ConfigureAwait(false);

                    // TODO : 

                    // Need To push Transaction For Each Qualified Campaign For Group Campaigns.
                    // For Each Campaign Transaction Id Must be Different.
                    // Need To Introduce ParentTransactionId to Identify That These Transaction are from a group Transaction.

                    if (IsGroupedCampaign(processedTransaction))
                    {
                        _logger.LogInformation($"Group campaign exists for {preFix}");
                        var parentTransactionId = processedTransaction.TransactionRequest.TransactionId;
                        var TransactionReferenceNumber = processedTransaction.TransactionRequest.TransactionDetail.RefNumber;

                        groupMatchedCampaigns = groupMatchedCampaigns
                                               .GroupBy(obj => obj.CampaignId)
                                               .Select(group => group.First())
                                               .ToList();
                        foreach (var campaign in groupMatchedCampaigns)
                        {
                            var rewardOption = campaign?.RewardOptions?.FirstOrDefault();
                            var groupCampaignTransaction = _processorService.GroupedCampaignTransaction(
                            new Models.GroupedCampaignTransaction()
                            {
                                TransactionId = parentTransactionId,
                                TransactionReferenceNumber = TransactionReferenceNumber,
                                CampaignId = campaign.CampaignId,
                                GroupedCampaignId = processedTransaction.TransactionRequest.Campaign.Id
                            }).GetAwaiter().GetResult();

                            _logger.LogInformation("{PreFix} ParentTransactionId {ParentTransactionId} For CampaignId {CampaignId} And BFLCampaignId {BFLCampaignId}", preFix, parentTransactionId, campaign.CampaignId, processedTransaction.TransactionRequest.Campaign.Id);
                            var newTransactionId = groupCampaignTransaction.Id;
                            _logger.LogInformation("{PreFix} ParentTransactionId {ParentTransactionId} And NewTransactionId {NewTransactionId} For CampaignId {CampaignId} And BFLCampaignId {BFLCampaignId}", preFix, parentTransactionId, newTransactionId, campaign.CampaignId, processedTransaction.TransactionRequest.Campaign.Id);
                            var newProcessedTransaction = new TransactionModel.ProcessedTransaction()
                            {
                                TransactionRequest = processedTransaction.TransactionRequest,
                                Customer = processedTransaction.Customer,
                                MatchedCampaigns = new List<TransactionModel.MatchedCampaign>() { campaign }
                            };

                            newProcessedTransaction.TransactionRequest.ParentTransactionId = parentTransactionId;

                            if (rewardOption != null && (rewardOption.RewardType.Equals("subscription", StringComparison.OrdinalIgnoreCase) || 
                                rewardOption.RewardType.Equals("paidsubscription", StringComparison.OrdinalIgnoreCase)))
                            {
                                //newProcessedTransaction.TransactionRequest.TransactionId (It will be as is)
                                _logger.LogInformation($"GroupCampaign for subscriptionTypes: {rewardOption.RewardType} and TransactionId: {newProcessedTransaction.TransactionRequest.TransactionId}");
                            }
                            else
                            {
                                newProcessedTransaction.TransactionRequest.TransactionId = newTransactionId;
                            }

                            PublishInKafka(newProcessedTransaction, OfferMapTopicName);
                            _logger.LogInformation($"{preFix} Final Qualified group campaign pushed in kafka  : {finalQualifiedCampaigns.MatchedCampaigns}");
                        }
                    }
                    else
                    {
                        PublishInKafka(finalQualifiedCampaigns, OfferMapTopicName);
                        _logger.LogInformation($"{preFix} Final Qualified campaign pushed in kafka : {finalQualifiedCampaigns.MatchedCampaigns}");
                    }

                    _logger.LogInformation("{PreFix} Data pushed in kafka : {finalQualifiedCampaigns}", preFix, JsonConvert.SerializeObject(finalQualifiedCampaigns));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error : {ex.Message}");
                    throw;
                }
            }
            void ManageNoCampaignQualified(TransactionModel.ProcessedTransaction processedTransaction)
            {
                _logger.LogInformation("{PreFix} No Campaign Quaified.", preFix);
                if (processedTransaction.TransactionRequest.EventId == "Spend")
                {
                    // TODO : need to discuss for the change Remove for crossLob function bfl-2653
                    InsertInTransactionReward(processedTransaction);
                }
                else
                {
                    InsertInCustomerEvent(processedTransaction);
                }
            }
            void InsertInCustomerEvent(TransactionModel.ProcessedTransaction processedTransaction)
            {
                var customerEvent = new CustomerModel.CustomerEvent()
                {
                    //CustomerVersionId = processedTransaction.Customer.CustomerVersionId,
                    Amount = processedTransaction.TransactionRequest.TransactionDetail.Amount,
                    EventCode = processedTransaction.TransactionRequest.EventId,
                    ChildEventCode = processedTransaction.TransactionRequest.ChildEventCode,
                    EventType = processedTransaction.TransactionRequest.EventId,
                    MobileNumber = processedTransaction.Customer.MobileNumber,
                    PaymentInstrument = processedTransaction.TransactionRequest.TransactionDetail.Payments?.Select(o => o.PaymentInstrument).ToList(),
                    TxnDateTime = processedTransaction.TransactionRequest.TransactionDetail.DateTime,
                    PaymentCategory = null,
                    CampaignId = null,
                    Id = null,
                    LoyaltyId = null,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    TransactionId = processedTransaction.TransactionRequest.TransactionId,
                    TransactionReferenceNumber = processedTransaction.TransactionRequest.TransactionDetail.RefNumber
                };
                Task.FromResult(_processorService.CreateCustomerEventAsync(customerEvent)).ConfigureAwait(false);
            }
            void InsertInTransactionReward(TransactionModel.ProcessedTransaction processedTransaction)
            {

                if (processedTransaction.TransactionRequest.EventId == "Spend")
                {
                    // insert into Transaction Reward With no Earning That means No Point And No Cashback.
                    var transactionReward = new MongoService.TransactionReward()
                    {
                        CustLoyaltyFraudStatus = processedTransaction.Customer.Flags.LoyaltyFraud,
                        Amount = (decimal)processedTransaction.TransactionRequest.TransactionDetail.Amount,
                        EventId = processedTransaction.TransactionRequest.EventId,
                        ChildEventCode = processedTransaction.TransactionRequest.ChildEventCode,
                        //CustLoyaltyId = processedTransaction.Customer.LoyaltyId,
                        Cashback = 0,
                        IsAccure = false,
                        IsScratched = false,
                        IsConvertedFromCashback = false,
                        ExpiryDate = DateTime.Now,
                        IssueDate = DateTime.Now,
                        IssueInState = "Direct",
                        IsLockExpires = false,
                        LockExpireDate = DateTime.Now,
                        MobileNumber = processedTransaction.Customer.MobileNumber,
                        //Lob = processedTransaction.TransactionRequest.LOB,// TODO : need to discuss for the change lob for crossLob function bfl-2653
                        Lob = processedTransaction.MatchedCampaigns != null && processedTransaction.MatchedCampaigns.Count > 0 ? processedTransaction.MatchedCampaigns.Select(s => s.LOB).FirstOrDefault() : processedTransaction.TransactionRequest.LOB,
                        Narration = String.Empty,
                        Points = 0,
                        TxnDate = processedTransaction.TransactionRequest.TransactionDetail.DateTime,
                        TxnId = processedTransaction.TransactionRequest.TransactionId,
                        TxnRefId = processedTransaction.TransactionRequest.TransactionDetail.RefNumber,
                        TxnType = 0,
                        PaymentCategory = processedTransaction.TransactionRequest.TransactionDetail.Type,
                        PaymentInstruments = processedTransaction.TransactionRequest.TransactionDetail.Payments?.Select(o => o.PaymentInstrument).ToList()
                    };
                    if (processedTransaction.TransactionRequest.TransactionDetail.Type == "bbps")
                    {
                        var billerDetail = _webUIDatabaseService.GetBillerDBEnumValues(preFix, processedTransaction.TransactionRequest.TransactionDetail.Biller.Category, processedTransaction.TransactionRequest.TransactionDetail.Biller.Id).GetAwaiter().GetResult().FirstOrDefault();
                        transactionReward.BillerName = billerDetail.Name;
                        transactionReward.BillerCategory = processedTransaction.TransactionRequest.TransactionDetail.Biller.Category;
                    }
                    else if (processedTransaction.TransactionRequest.TransactionDetail.Type == "p2m")
                    {
                        transactionReward.MerchantId = processedTransaction.TransactionRequest.TransactionDetail.MerchantOrDealer?.Id;
                        transactionReward.MerchantName = processedTransaction.TransactionRequest.TransactionDetail.MerchantOrDealer?.Category;
                        transactionReward.GrpMerchantId = processedTransaction.TransactionRequest.TransactionDetail.MerchantOrDealer?.GroupId;
                        transactionReward.MerchantSource = processedTransaction.TransactionRequest.TransactionDetail.MerchantOrDealer?.Source;
                    }
                    Task.FromResult(_processorService.CreateTransactionReward(transactionReward)).ConfigureAwait(false);
                }
            }
            static bool IsGroupedCampaign(TransactionModel.ProcessedTransaction processedTransaction) => processedTransaction.TransactionRequest.Campaign != null && processedTransaction.TransactionRequest.Campaign.RewardedFlg && !String.IsNullOrEmpty(processedTransaction.TransactionRequest.Campaign.Id);

            List<CampaignModel.EarnCampaign> GetUnlockReferralCampaigns(List<CampaignModel.EarnCampaign> qualifiedCampaigns, ProcessedTransaction processedTransaction)
            {
                var unlockReferralCampaigns = qualifiedCampaigns.Where(x => x.IsUnlockCampaign(processedTransaction).IsUnlock == true && x.Filter.IsRefferalProgram).ToList();
                return ApplySubscriptionFilterForUnlock(unlockReferralCampaigns, processedTransaction);
            }

            List<CampaignModel.EarnCampaign> GetDirectCampaigns(List<CampaignModel.EarnCampaign> qualifiedCampaigns, ProcessedTransaction processedTransaction)
            {
                var directCampaigns = qualifiedCampaigns.Where(x => x.IsDirectCampaign() == true && x.Status == "ACTIVE" && x.EndDate >= DateTime.Now).ToList();
                return ApplySubscriptionFilterForDirectAndLock(directCampaigns, processedTransaction);
            }

            List<CampaignModel.EarnCampaign> ApplySubscriptionFilterForDirectAndLock(List<CampaignModel.EarnCampaign> campaigns, ProcessedTransaction processedTransaction)
            {
                var subscriptionCampaignExists = campaigns.Any(o => o.RewardOption.Any(r => (String.Equals(r.RewardType, "Subscription", StringComparison.OrdinalIgnoreCase) || String.Equals(r.RewardType, "PaidSubscription", StringComparison.OrdinalIgnoreCase))));

                if (subscriptionCampaignExists)
                {
                    //var subscriptionTempRewards = _processorService.GetSubscriptionTempTransactions(processedTransaction.Customer.MobileNumber);
                    //if (subscriptionTempRewards == null || !subscriptionTempRewards.Any())
                    //{
                        campaigns = GetSubscriptionCampaigns(campaigns);
                    //}
                    //else
                    //{
                    //    campaigns = GetNonSubscriptionCampaigns(campaigns);
                    //}
                }
                return campaigns;
            }

            List<CampaignModel.EarnCampaign> ApplySubscriptionFilterForUnlock(List<CampaignModel.EarnCampaign> campaigns, ProcessedTransaction processedTransaction)
            {
                var subscriptionCampaignExists = campaigns.Any(o => o.RewardOption.Any(r => String.Equals(r.RewardType, "Subscription", StringComparison.OrdinalIgnoreCase)));

                if (subscriptionCampaignExists)
                {
                    var subscriptionTempRewards = _processorService.GetSubscriptionTempTransactions(processedTransaction.Customer.MobileNumber);
                    if (subscriptionTempRewards == null || !subscriptionTempRewards.Any())
                    {
                        campaigns = GetNonSubscriptionCampaigns(campaigns);
                    }
                    else
                    {
                        campaigns = GetSubscriptionCampaigns(campaigns);
                    }
                }
                return campaigns;
            }

            List<CampaignModel.EarnCampaign> GetUnlockCampaigns(List<CampaignModel.EarnCampaign> qualifiedCampaigns, ProcessedTransaction processedTransaction)
            {
                var unlockCampaigns = qualifiedCampaigns.Where(x => x.IsUnlockCampaign(processedTransaction).IsUnlock == true && x.Filter.IsRefferalProgram == false).ToList();
                return ApplySubscriptionFilterForUnlock(unlockCampaigns, processedTransaction);
            }

            List<CampaignModel.EarnCampaign> GetLockCampaigns(List<CampaignModel.EarnCampaign> qualifiedCampaigns, ProcessedTransaction processedTransaction)
            {
                var lockCampaigns = qualifiedCampaigns.Where(x => x.IsLockCampaign(processedTransaction) == true && x.Status == "ACTIVE" && x.EndDate >= DateTime.Now && x.RewardCriteria.WithLockUnlock?.LockEvent?.EventName == processedTransaction.TransactionRequest.EventId).ToList();
                return ApplySubscriptionFilterForDirectAndLock(lockCampaigns, processedTransaction);
            }

            List<CampaignModel.EarnCampaign> GetNonSubscriptionCampaigns(List<CampaignModel.EarnCampaign> earnCampaigns)
            {
                return earnCampaigns.Where(o =>
                {
                    var subscriptionRewardType = o.RewardOption.Where(r => (String.Equals(r.RewardType, "Subscription", StringComparison.OrdinalIgnoreCase) || String.Equals(r.RewardType, "PaidSubscription", StringComparison.OrdinalIgnoreCase))).ToList();
                    if (subscriptionRewardType.Any())
                    {
                        return false;
                    }
                    return true;
                }).ToList();
            }

            List<CampaignModel.EarnCampaign> GetSubscriptionCampaigns(List<CampaignModel.EarnCampaign> earnCampaigns)
            {
                return earnCampaigns.Where(o =>
                {
                    var subscriptionRewardType = o.RewardOption.Where(r => (String.Equals(r.RewardType, "Subscription", StringComparison.OrdinalIgnoreCase) || String.Equals(r.RewardType, "PaidSubscription", StringComparison.OrdinalIgnoreCase))).ToList();
                    if (subscriptionRewardType.Any())
                    {
                        return true;
                    }
                    return false;
                }).ToList();
            }

            //  static bool IsGroupedCampaign(TransactionModel.ProcessedTransaction processedTransaction) => processedTransaction.TransactionRequest.Campaign != null && processedTransaction.TransactionRequest.Campaign.RewardedFlg && !String.IsNullOrEmpty(processedTransaction.TransactionRequest.Campaign.Id);
        }

        private string GetLogInformation(string transactionMobileNumber, string transactionReferenceNumber)
        {
            return $"TransactionMobileNumber : {transactionMobileNumber}, TransactionReferenceNumber : {transactionReferenceNumber} ::: ";
        }
        #region Private Section
        private IEnumerable<CampaignModel.EarnCampaign> ApplySubscriptionTypeFilter(IEnumerable<CampaignModel.EarnCampaign> campaigns, Domain.Models.CustomerModel.Customer customer, Campaign transactionCampaign)
        {
            IEnumerable<CampaignModel.EarnCampaign> quaifiedCampaigns = new List<CampaignModel.EarnCampaign>();
            foreach (var campaign in campaigns)
            {
                #region "For subscription rewards, when RewardFlag is true, no rewards will be given. Only campaigns with RewardFlag set to false will be considered for rewarding subscriptions." Check
                //var hasSubscriptionReward = campaign.RewardOption.Any(ro => ro.RewardType.Equals("subscription", StringComparison.OrdinalIgnoreCase));
                //if (transactionCampaign != null)
                //{
                //    if (transactionCampaign.RewardedFlg && hasSubscriptionReward)
                //    {
                //        _logger.LogInformation("Campaign ID: {CampaignId} - RewardFlag is true and has subscription reward. No rewards will be given.", campaign.Id);
                //        return quaifiedCampaigns;
                //    }
                //}
                #endregion
                var campaignFilter = campaign.Filter;
                if (campaignFilter.IsAnySubscriptionType)
                {
                    campaign.SubscriptionTypes = new List<string>();
                    quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                }
                else
                {
                    bool isPaidSubscriptionCampaign = campaign.RewardOption.Any(ro => ro.RewardType.Equals("paidsubscription", StringComparison.OrdinalIgnoreCase));
                    bool isEligibleCustomer = customer.SubscriptionType.Equals("Regular", StringComparison.OrdinalIgnoreCase) ||
                                              customer.SubscriptionType.Equals("Trial", StringComparison.OrdinalIgnoreCase);

                    if (isPaidSubscriptionCampaign && isEligibleCustomer)
                    {
                        _logger.LogInformation($"Campaign ID: {campaign.Id} - Selected for 'Paid Subscription' Reward.");
                        quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                    }
                    else
                    {
                        var isCampaignForCustomerMembershipType = campaign.SubscriptionTypes.Contains(customer.SubscriptionType, StringComparer.OrdinalIgnoreCase);
                        if (isCampaignForCustomerMembershipType)
                        {
                            quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                        }
                    }
                }
            }

            return quaifiedCampaigns;
        }
        public IEnumerable<CampaignModel.EarnCampaign> ApplySegmantFilter(IEnumerable<CampaignModel.EarnCampaign> campaigns, CustomerModel.Customer customer)
        {
            IEnumerable<CampaignModel.EarnCampaign> quaifiedCampaigns = new List<CampaignModel.EarnCampaign>();
            //_logger.LogInformation("=========================================================================================================================");
            //_logger.LogInformation("{PreFix} Campaigns Before Customer Segment Apply {campaigns}", preFix, campaigns == null ? null : JsonConvert.SerializeObject(campaigns));
            _logger.LogInformation("{PreFix} Campaigns Before Customer Segment Apply {campaigns}", preFix, campaigns == null ? null : string.Join(",", campaigns.Select(c => c.Id)));
            _logger.LogInformation("{PreFix} Campaigns count Before Customer Segment Apply {campaigns}", preFix, campaigns == null ? null : campaigns.Count().ToString());
            foreach (var campaign in campaigns)
            {

                _logger.LogInformation("{PreFix} Campaigns for checking Customer Segment  {campaigns}", preFix, campaign == null ? null : JsonConvert.SerializeObject(campaign));
                // if campaign doesn't contains any segment (campaign.segment==null or campaign.segment.count == 0)
                // then qualify the campaign
                // else
                // check if customer falls in the campaign segments if true
                // then qualify the campaign.
                var campaignSegment = campaign.SegmentType == CampaignEnum.No.ToString() ? null : JsonConvert.SerializeObject(campaign?.Segment);
                _logger.LogInformation("{PreFix} Campaign Segment : {campaignSegment}", preFix, campaignSegment);
                // if (campaign.Segment == null || campaign.Segment?.Count <= 0)
                if (campaign.SegmentType == CampaignEnum.No.ToString())
                {
                    quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                }
                else if (campaign.SegmentSelect == CampaignEnum.Internal.ToString())
                {
                    // customer falls in any segment
                    //if (customer.Segments?.Count > 0)
                    //{
                    //var matchedSegment = campaign.Segment.Any(o => customer.Segments.Find(p => p.Code == o) != null);
                    var campaignSegmentCode = campaign.Segment.Select(o => o);
                    _logger.LogInformation("{PreFix} CampaignSegmentCode : {CampaignSegmentCode}  ", preFix, JsonConvert.SerializeObject(campaignSegmentCode));

                    var inQuery = "'" + string.Join("', '", campaignSegmentCode) + "'";
                    var query = @" WHERE CS.MobileHash = '" + customer.MobileNumber + "' AND S.SegmentCode IN (" + inQuery + ")";

                    _logger.LogInformation("{PreFix} Query : {query}", preFix, query);
                    var matchedSegmentCount = _webUIDatabaseService.GetCustomerSegmentCount(query);
                    _logger.LogInformation("{PreFix} MatchedSegmentCount : {MatchedSegmentCount}", preFix, matchedSegmentCount);

                    if (matchedSegmentCount > 0 && campaign.SegmentType == CampaignEnum.Inclusive.ToString())
                    {
                        _logger.LogInformation("{PreFix} Campaign Adding To Qualified Campaign.", preFix);
                        quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                    }
                    else if (matchedSegmentCount <= 0 && campaign.SegmentType == CampaignEnum.Exclusive.ToString())
                    {
                        _logger.LogInformation("{PreFix} Campaign Adding To Qualified Campaign.", preFix);
                        quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                    }
                    //}
                }


                else if (campaign.SegmentSelect == CampaignEnum.External.ToString())
                {
                    ValidateExternalSegmentRequest validateExternalSegmentrequest = new ValidateExternalSegmentRequest();
                    validateExternalSegmentrequest.MobileNo = customer.MobileNumber;
                    var externalsegment = GetExternalSegmentValidate(validateExternalSegmentrequest);
                    if (externalsegment.Result.Data.CustomerSegments != null)
                    {
                        var externalSegmentlist = externalsegment.Result.Data.CustomerSegments.SegmentList.ToList();
                        _logger.LogInformation($"{preFix} External Segment List: {JsonConvert.SerializeObject(externalSegmentlist)}");
                        if (campaign.Segment.Any(segment => externalSegmentlist.Contains(segment)) && campaign.SegmentType == CampaignEnum.Inclusive.ToString())
                        {
                            _logger.LogInformation($"campaign qualified for externalsegment");
                            quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                        }
                        else if (!externalSegmentlist.Any(segment => campaign.Segment.Contains(segment)) && campaign.SegmentType == CampaignEnum.Exclusive.ToString())
                        {
                            _logger.LogInformation($"campaign  qualified for externalsegment ");
                            quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                        }

                    }

                    else if (externalsegment.Result.Data.CustomerSegments == null && campaign.SegmentType == CampaignEnum.Exclusive.ToString())
                    {
                        _logger.LogInformation("{PreFix} Campaign Adding To Qualified Campaign.", preFix);
                        quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                    }

                }
            }
            //_logger.LogInformation("{PreFix} Campaigns After Customer Segment Apply {campaigns}", preFix, quaifiedCampaigns == null ? null : JsonConvert.SerializeObject(quaifiedCampaigns));
            _logger.LogInformation("{PreFix} Campaigns After Customer Segment Apply {campaigns}", preFix, quaifiedCampaigns == null ? null : string.Join(",", quaifiedCampaigns.Select(c => c.Id)));
            //_logger.LogInformation("=========================================================================================================================");
            return quaifiedCampaigns;
        }
        private (IEnumerable<CampaignModel.EarnCampaign>, IEnumerable<CampaignModel.EarnCampaign>, IEnumerable<CampaignModel.EarnCampaign>) ApplyAdvancedFilter(IEnumerable<CampaignModel.EarnCampaign> campaigns, TransactionModel.ProcessedTransaction processedTransaction, CustomerModel.CustomerSummary customerSummary, CustomerModel.Customer customer)
        {
            _logger.LogInformation("{PreFix} Applying advance filter :", preFix);
            // var enumvalues = _processorService.GetEnumValues().GetAwaiter().GetResult();
            _logger.LogInformation("{PreFix} Campaign Count before SubscriptionType : {count}", preFix, campaigns.Count());
            campaigns = ApplySubscriptionTypeFilter(campaigns, processedTransaction.Customer, processedTransaction.TransactionRequest.Campaign);
            //_logger.LogInformation("{PreFix} Campaign After SubscriptionType : {campaigns}", preFix, JsonConvert.SerializeObject(campaigns));
            _logger.LogInformation("{PreFix} Campaign After SubscriptionType : {campaigns}", preFix, string.Join(",", campaigns.Select(c => c.Id)));
            // Segment Filter
            campaigns = ApplySegmantFilter(campaigns, processedTransaction.Customer);
            _logger.LogInformation($"{preFix} Campaign After Segment :{campaigns.Count()}");
            // Customer Status Check
            campaigns = ApplyCustomerStatusFilter(campaigns, processedTransaction/*, enumvalues*/);
            _logger.LogInformation($"{preFix} Campaign After CustomerStatus :{campaigns.Count()}");

            // Referrer Customer Status Check
            if (processedTransaction.TransactionRequest?.TransactionDetail?.ReferAndEarn != null && processedTransaction.TransactionRequest.TransactionDetail.ReferAndEarn.IsReferAndEarn)
            {
                campaigns = ApplyReferrerCustomerStatusFilter(campaigns, processedTransaction/*, enumvalues*/);
                _logger.LogInformation($"{preFix} Campaign After Referrer CustomerStatus :{campaigns.Count()}");
            }

            // RMS Check
            campaigns = ApplyRMSFilter(campaigns, processedTransaction.Customer, customerSummary, customer);
            _logger.LogInformation($"{preFix} Campaign After RMS :{campaigns.Count()}");

            campaigns = ApplyInstallationSourceFilter(campaigns, processedTransaction.Customer);
            _logger.LogInformation($"{preFix} Campaign After InstallationSource :{campaigns.Count()}");

            //campaigns = await ApplyLobBudgetCappingFilter(campaigns, processedTransaction);

            // Merchant And Biller Filter
            var merchantBillerResponse = ApplyMerchantAndBillerFilter(campaigns, processedTransaction);
            _logger.LogInformation($"{preFix} Qualified Campaign After MerchantBiller: {JsonConvert.SerializeObject(merchantBillerResponse.Item1.Any())}");
            //_logger.LogInformation($"Cumulative Campaign After RMS : {JsonConvert.SerializeObject(campaigns)}");

            //campaigns = ApplyOncePerCampaignFilter(merchantBillerResponse.Item1, processedTransaction, customerMobileCampaignMappings);


            return merchantBillerResponse;

        }

        private async Task<List<EarnCampaign>> ApplyLobBudgetCappingFilter(IEnumerable<EarnCampaign> campaigns, ProcessedTransaction transaction)
        {
            IEnumerable<CampaignModel.EarnCampaign> quaifiedCampaigns = new List<CampaignModel.EarnCampaign>();
            foreach (var campaign in campaigns)
            {
                if (!campaign.ExcludeFromBudgetCapping)
                {
                    //Budget capping not applicable
                    quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                    _logger.LogInformation("{prefix} Campaign Id {campaignId} excluded from budget capping.", preFix, campaign.Id);
                }
                else
                {
                    var lob = campaign.LOB;
                    // TODO: Hit GetAvailableLobMonthlyBudgetByMobileNumber
                    try
                    {
                        var response = await _loanMonthlyBudgetCappingApiClient.LobMonthlyBudgetCustomerSummary_GetAvailableBudgetAsync(new LobMonthlyBudgetCustomerSummaryRequest
                        {
                            Company = campaign.CompanyCode,
                            Lob = campaign.LOB,
                            MobileNumber = transaction.Customer.MobileNumber,
                            Month = transaction.TransactionRequest.TransactionDetail.DateTime.Month,
                            Year = transaction.TransactionRequest.TransactionDetail.DateTime.Year
                        }).ConfigureAwait(false);

                        if (response.StatusCode == 2000)
                        {
                            bool isBudgetAvailable = response.Data.AvailableBudget > 0;
                            if (isBudgetAvailable)
                            {
                                quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                            }
                            else
                            {
                                _logger.LogInformation("{prefix} Campaign Id : {campaign} budget exhausted for {mobileNumber} for LOB : {lob} for {month}-{year}.", preFix, response.Message, campaign.Id, transaction.Customer.MobileNumber, campaign.LOB, transaction.TransactionRequest.TransactionDetail.DateTime.Month, transaction.TransactionRequest.TransactionDetail.DateTime.Year);
                            }
                        }
                        else
                        {
                            _logger.LogInformation("{prefix} {message}", preFix, response.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message, ex);
                    }
                }
            }
            return quaifiedCampaigns.ToList();
        }

        private IEnumerable<CampaignModel.EarnCampaign> ApplyOncePerCampaignFilter(IEnumerable<CampaignModel.EarnCampaign> campaigns, TransactionModel.ProcessedTransaction processedTransaction, List<MongoService.CustomerMobileCampaignMapping> customerMobileCampaignMappings)
        {
            _logger.LogInformation("==================================={PreFix} ApplyOncePerCampaignFilter Start ===============================", preFix);
            //_logger.LogInformation("{PreFix} Incomming Campaigns {campaigns}", preFix, JsonConvert.SerializeObject(campaigns));
            _logger.LogInformation("{PreFix} Incomming Campaigns {campaigns}", preFix, string.Join(",", campaigns.Select(c => c.Id)));
            _logger.LogInformation("{PreFix} Incomming CustomerMobileCampaignMapping {customerMobileCampaignMapping}", preFix, JsonConvert.SerializeObject(customerMobileCampaignMappings));

            IEnumerable<CampaignModel.EarnCampaign> quaifiedCampaigns = new List<CampaignModel.EarnCampaign>();

            var mobileNumber = processedTransaction.Customer.MobileNumber;
            _logger.LogInformation("{PreFix} MobileNumber {mobileNumber}", preFix, mobileNumber);
            var transactionEventCode = processedTransaction.TransactionRequest.EventId;
            _logger.LogInformation("{PreFix} TransactionEventCode {transactionEventCode}", preFix, transactionEventCode);

            foreach (var campaign in campaigns)
            {
                _logger.LogInformation($"Incoming campaign {JsonConvert.SerializeObject(campaign)}");
                var campaignId = campaign.Id;

                _logger.LogInformation("--------------------------- Processing Start For Campaign Id {campaignId} -------------------------------", campaignId);
                _logger.LogInformation("{PreFix} CampaignId : {campaignId}", preFix, campaignId);
                var filterSetting = campaign.Filter;
                _logger.LogInformation("{PreFix} FilterSetting {filterSetting}", preFix, JsonConvert.SerializeObject(filterSetting));
                var isOncePerCampaign = campaign.OncePerCampaign;
                _logger.LogInformation("{PreFix} IsOncePerCampaign {isOncePerCampaign}", preFix, isOncePerCampaign);
                var isOnceInLifeTime = campaign.OnceInLifeTime.Value;
                _logger.LogInformation("{PreFix} IsOnceInLifeTime : {isOnceInLifeTime}", preFix, isOnceInLifeTime);
                var offerType = campaign.OfferType;
                var rewardIssuance = campaign.RewardCriteria.RewardIssuance;

                if (!isOncePerCampaign && !isOnceInLifeTime)
                {
                    quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                    _logger.LogInformation($"MobileNumber ({mobileNumber}) => CampaignId ({campaignId}) => :: Default Mode Applied.");
                    continue;
                }

                IEnumerable<MongoService.CustomerMobileCampaignMapping> customerNotRewardedInCampaigns = null;
                _logger.LogInformation("{PreFix} CustomerNotRewardedInCampaigns {customerNotRewardedInCampaigns}", preFix, customerNotRewardedInCampaigns == null ? null : JsonConvert.SerializeObject(customerNotRewardedInCampaigns));
                if (isOncePerCampaign)
                {
                    customerNotRewardedInCampaigns = customerMobileCampaignMappings.Where(o => o.CampaignId == campaign.Id);
                    _logger.LogInformation("{PreFix} IsOncePerCampaign : CustomerNotRewardedInCampaigns {customerNotRewardedInCampaigns}", preFix, JsonConvert.SerializeObject(customerNotRewardedInCampaigns));
                    _logger.LogInformation("{PreFix} => CampaignId ({campaignId}) => IsOncePerCampaign : ", preFix, isOncePerCampaign);

                }
                else if (isOnceInLifeTime)
                {
                    customerNotRewardedInCampaigns = customerMobileCampaignMappings.Where(o => campaign.OnceInLifeTime.Tags.Intersect(o.OnceInLifeTimeTags).Any());
                    _logger.LogInformation("{PreFix} IsOnceInLifeTime : CustomerNotRewardedInCampaigns {customerNotRewardedInCampaigns}", preFix, JsonConvert.SerializeObject(customerNotRewardedInCampaigns));
                    _logger.LogInformation("{PreFix} => CampaignId ({campaignId}) => IsOnceInLifeTime : ", preFix, isOnceInLifeTime);
                }
                if (customerNotRewardedInCampaigns != null && customerNotRewardedInCampaigns.Any())
                {
                    ///in case of non generic isLock=true
                    string transactionChildEventCode = processedTransaction.TransactionRequest.ChildEventCode;
                    string lockChildEventCode = GetCampaignLockChildEventCode(campaign, transactionChildEventCode);
                    string unlockChildEventCode = GetCampaignUnLockChildEventCode(campaign);

                    _logger.LogInformation($"{preFix} lock Child eventCode: {lockChildEventCode}");
                    _logger.LogInformation($"{preFix} unlock ChildEventCode : {unlockChildEventCode}");
                    var res1 = filterSetting.IsLock && ((string.Equals(offerType, OfferTypeEnum.GENERAL_OFFERS, StringComparison.OrdinalIgnoreCase) && string.Equals(rewardIssuance, RewardIssuanceEnum.WITH_LOCK_UNLOCK, StringComparison.OrdinalIgnoreCase) && string.Equals(campaign.RewardCriteria?.WithLockUnlock?.LockEvent?.EventName, transactionEventCode, StringComparison.OrdinalIgnoreCase)) || transactionEventCode == filterSetting.LockEvent);
                    _logger.LogInformation($"{preFix} filterSetting.IsUnlock && transactionEventCode == filterSetting.LockEvent =" + res1);

                    if (filterSetting.IsLock && ((string.Equals(offerType, OfferTypeEnum.GENERAL_OFFERS, StringComparison.OrdinalIgnoreCase) && string.Equals(rewardIssuance, RewardIssuanceEnum.WITH_LOCK_UNLOCK, StringComparison.OrdinalIgnoreCase) && string.Equals(campaign.RewardCriteria?.WithLockUnlock?.LockEvent?.EventName, transactionEventCode, StringComparison.OrdinalIgnoreCase)) || transactionEventCode == filterSetting.LockEvent)
                        && ((String.IsNullOrEmpty(transactionChildEventCode) ? true : (lockChildEventCode == transactionChildEventCode) ? true : false)))
                    {

                        var _customerNotRewardedInCampaigns = customerNotRewardedInCampaigns?.Where(o => (o.IsLock != null && !Convert.ToBoolean(o.IsLock)) && (o.IsUnlock != null && !Convert.ToBoolean(o.IsUnlock)));
                        _logger.LogInformation(" {PreFix} => CampaignId ({campaignId}) => FilterSetting.IsLock : ", preFix, filterSetting.IsLock);
                        //Not exist is  in Customer mobile mapping 
                        if ((_customerNotRewardedInCampaigns.Count() > 0))
                        {
                            _logger.LogInformation("{PreFix} => CampaignId ({campaignId}) => CustomerNotRewardedInCampaigns:", preFix, JsonConvert.SerializeObject(customerNotRewardedInCampaigns));
                            quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                            continue;
                        }
                    }
                    var res = filterSetting.IsUnlock && ((string.Equals(offerType, "PaymentHybrid", StringComparison.OrdinalIgnoreCase) && string.Equals(rewardIssuance, "withlockunlock", StringComparison.OrdinalIgnoreCase)) || transactionEventCode == filterSetting.UnlockEvent);
                    _logger.LogInformation($"{preFix} filterSetting.IsUnlock && transactionEventCode == filterSetting.UnlockEvent =" + res);
                    if ((filterSetting.IsUnlock && ((string.Equals(offerType, "PaymentHybrid", StringComparison.OrdinalIgnoreCase) || (string.Equals(offerType, "Hybrid", StringComparison.OrdinalIgnoreCase)) && string.Equals(rewardIssuance, "withlockunlock", StringComparison.OrdinalIgnoreCase)) || transactionEventCode == filterSetting.UnlockEvent))
                        && ((String.IsNullOrEmpty(transactionChildEventCode) ? true : (unlockChildEventCode == transactionChildEventCode) ? true : false)))
                    {
                        var _customerNotRewardedInCampaigns = customerNotRewardedInCampaigns?.Where(o => (o.IsLock != null && Convert.ToBoolean(o.IsLock)) && (o.IsUnlock != null && !Convert.ToBoolean(o.IsUnlock)));
                        _logger.LogInformation("{PreFix} => CampaignId ({campaignId}) => FilterSetting.IsUnlock : {IsUnlock}", preFix, campaignId, filterSetting.IsUnlock);
                        if ((_customerNotRewardedInCampaigns.Count() > 0))
                        {
                            _logger.LogInformation("{PreFix} => CampaignId ({campaignId}) => CustomerNotRewardedInCampaigns: {CustomerNotRewardedInCampaigns}", preFix, campaignId, JsonConvert.SerializeObject(customerNotRewardedInCampaigns));
                            quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                            continue;
                        }
                    }
                }


                _logger.LogInformation("{PreFix} CustomerNotRewardedInCampaigns {customerNotRewardedInCampaigns}", preFix, customerNotRewardedInCampaigns == null ? null : JsonConvert.SerializeObject(customerNotRewardedInCampaigns));

                if (customerNotRewardedInCampaigns == null)
                {
                    quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                    _logger.LogInformation("{PreFix} => CampaignId ({campaignId}) => Null Condition Matched => CustomerNotRewardedInCampaigns:", preFix, JsonConvert.SerializeObject(customerNotRewardedInCampaigns));
                    _logger.LogInformation("--------------------------- Processing End For Campaign Id {campaignId} -------------------------------", campaignId);
                    continue;
                }
                if (!(customerNotRewardedInCampaigns.Count() > 0))
                {
                    quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                    _logger.LogInformation($"MobileNumber ({mobileNumber}) => CampaignId ({campaignId}) => Blank Array Matched => CustomerNotRewardedInCampaigns:{JsonConvert.SerializeObject(customerNotRewardedInCampaigns)} => Blank Array Condition Matched.");
                    _logger.LogInformation("--------------------------- Processing End For Campaign Id {campaignId} -------------------------------", campaignId);
                }

            }
            _logger.LogInformation("=================================== ApplyOncePerCampaignFilter End ===============================");
            return quaifiedCampaigns;
            string GetCampaignLockChildEventCode(CampaignModel.EarnCampaign campaign, string childEventCode)
            {
                var lockEvent = campaign.RewardCriteria?.WithLockUnlock?.LockEvent;
                if (!string.IsNullOrEmpty(lockEvent?.Event?.GetGenericActivity()?.ChildEventCode))
                {
                    return lockEvent?.Event?.GetGenericActivity()?.ChildEventCode;
                }
                return string.Empty;
            }
            string GetCampaignUnLockChildEventCode(CampaignModel.EarnCampaign campaign)
            {
                var unlockEvent = campaign.RewardCriteria?.WithLockUnlock?.UnlockEvent;
                if (!string.IsNullOrEmpty(unlockEvent?.Event?.GetGenericActivity()?.ChildEventCode))
                {
                    return unlockEvent?.Event?.GetGenericActivity()?.ChildEventCode;
                }
                return string.Empty;
            }
        }

        private (IEnumerable<CampaignModel.EarnCampaign>, IEnumerable<CampaignModel.EarnCampaign>, IEnumerable<CampaignModel.EarnCampaign>) ApplyMerchantAndBillerFilter(IEnumerable<CampaignModel.EarnCampaign> campaigns, TransactionModel.ProcessedTransaction transaction)
        {
            const StringComparison _strOrd = StringComparison.OrdinalIgnoreCase;



            if (!string.Equals(transaction.TransactionRequest.EventId, "spend", _strOrd))
            {
                // since only spend with p2p,p2m,and bbps contains merchant or biller 
                // info we don't check further if eventtype is not spend.
                _logger.LogInformation($"This is not spend event transaction so no need of marchant filter Event is :{transaction.TransactionRequest.EventId}");
                _logger.LogInformation("{PreFix} Campaigns after ApplyMerchantAndBillerFilter Processing : {campaigns}", preFix, campaigns == null ? null : JsonConvert.SerializeObject(campaigns));

                return (campaigns, new List<CampaignModel.EarnCampaign>(), new List<CampaignModel.EarnCampaign>());
            }

            _logger.LogInformation("============================================================================================================");
            _logger.LogInformation("===================== {PreFix} ApplyMerchantAndBillerFilter Processing Start =============================", preFix);
            IEnumerable<CampaignModel.EarnCampaign> quaifiedCampaigns = new List<CampaignModel.EarnCampaign>();
            IEnumerable<CampaignModel.EarnCampaign> quaifiedCumulativeCampaigns = new List<CampaignModel.EarnCampaign>();
            IEnumerable<CampaignModel.EarnCampaign> qualifiedTripleRewardCampaigns = new List<CampaignModel.EarnCampaign>();

            _logger.LogInformation("{PreFix} Campaigns Before Processing : {campaigns}", preFix, campaigns == null ? null : JsonConvert.SerializeObject(campaigns));
            // if the transaction eventtype is spend
            // qualify only campaigns with below conditions
            // offertype=pyament and rewardissuance=direct or
            // offertype=hybrid and rewardissuance=withlockunlock in unlock event section

            var _txnDetail = transaction?.TransactionRequest?.TransactionDetail;
            foreach (var campaign in campaigns)
            {

                _logger.LogInformation("============================================================================");
                _logger.LogInformation("Processing start for ::{PreFix} Current Campaign Id :: {campaign}", preFix, campaign.Id);
                _logger.LogInformation("{PreFix} Current Campaign : {campaign}", preFix, JsonConvert.SerializeObject(campaign));

                var _offerType = campaign.OfferType; // Generic, Triple Reward
                _logger.LogInformation("{PreFix} CampaignId:{Campaign} OfferType: {OfferType} ", preFix, campaign.Id, _offerType);
                var _rewardIssuance = campaign.RewardCriteria.RewardIssuance; // Direct, WithLockUnlock
                _logger.LogInformation($"RewardIssuance : {_rewardIssuance}");
                var _txnPymtCat = _txnDetail.Type;
                _logger.LogInformation($"TxnPymtCat : {_txnPymtCat}");
                var _txnPymtInstrmt = _txnDetail.Payments.Select(o => o.PaymentInstrument);
                _logger.LogInformation($"TxnPymtInstrmt : {JsonConvert.SerializeObject(_txnPymtInstrmt)}");

                if (string.Equals(_offerType, OfferTypeEnum.TRIPLE_REWARD, _strOrd) && string.Equals(_rewardIssuance, RewardIssuanceEnum.AS_SCRATCH_CARD, _strOrd))
                {
                    var asScratchCard = campaign.RewardCriteria.AsScratchCard;

                    if (!String.Equals(asScratchCard.TransactionType, "Single", _strOrd))
                    {
                        _logger.LogInformation($"TripleReward => Transaction Type is not Single.");
                        _logger.LogInformation("Processing end for ::{PreFix} Current Campaign Id :: {campaign}", preFix, campaign.Id);
                        continue;
                    }

                    var _pymtCats = asScratchCard.Duration.PaymentCategories;

                    if (!_pymtCats.Contains(_txnPymtCat, StringComparer.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation($"Payment Categories dosenot matched : {JsonConvert.SerializeObject(_pymtCats)}");
                        _logger.LogInformation("Processing end for ::{PreFix} Current Campaign Id :: {campaign}", preFix, campaign.Id);
                        continue;
                    }

                    // here we check if any of the transaction paymentinstrument exists in campaign paymentinstrument
                    var _pymtInstrmt = asScratchCard.Duration.PaymentInstruments;
                    bool _txnPymtInstrmtHasMatch = _pymtInstrmt.Select(s => s).Intersect(_txnPymtInstrmt, StringComparer.OrdinalIgnoreCase).Any();

                    if (!_txnPymtInstrmtHasMatch)
                    {
                        _logger.LogInformation($"Payment Instrument dosenot matched : {JsonConvert.SerializeObject(_pymtInstrmt)}");
                        _logger.LogInformation("Processing end for ::{PreFix} Current Campaign Id :: {campaign}", preFix, campaign.Id);
                        continue;
                    }

                    if (string.Equals(_txnPymtCat, "p2m", _strOrd))
                    {
                        // Add in qualifiedTripleRewardCampaigns
                        // TODO : need to discuss for the change Remove for crossLob function bfl-2653
                        if (string.Equals(transaction.TransactionRequest.EventId, EventEnum.SPEND, _strOrd))
                        {
                            var tripleRewardMerchants = _merchantMasterService.GetMerchantMasterValues(new MerchantEnumRequest()
                            {
                                Category = _txnDetail.MerchantOrDealer.Category,
                                GroupMerchantId = _txnDetail.MerchantOrDealer.GroupId,
                                MerchantId = _txnDetail.MerchantOrDealer.Id,
                                Source = _txnDetail.MerchantOrDealer.Source,
                                TripleReward = 1,
                                MerchantType = campaign.RewardCriteria?.AsScratchCard?.Duration?.Merchant?.MerchantType
                            });

                            if (tripleRewardMerchants == null)//|| !tripleRewardMerchants.Any())
                            {
                                var responseFlag = _processorService.ValidateMerchantSegment(asScratchCard.Duration.Merchant, _txnDetail);
                                if (responseFlag)
                                {
                                    qualifiedTripleRewardCampaigns = qualifiedTripleRewardCampaigns.Append(campaign);
                                    _logger.LogInformation("Processing end for ::{PreFix} Current Campaign Id :: {campaign}", preFix, campaign.Id);
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            var responseFlag = _processorService.ValidateMerchantSegment(asScratchCard.Duration.Merchant, _txnDetail);
                            if (responseFlag)
                            {
                                quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                                _logger.LogInformation("Processing end for ::{PreFix} Current Campaign Id :: {campaign}", preFix, campaign.Id);
                                continue;
                            }
                        }
                    }
                }

                if (string.Equals(_offerType, OfferTypeEnum.GENERAL_OFFERS, _strOrd)
                    && string.Equals(_rewardIssuance, RewardIssuanceEnum.DIRECT, _strOrd)
                    && string.Equals(campaign?.RewardCriteria?.Direct?.EventName, EventEnum.SPEND, _strOrd))
                {
                    _logger.LogInformation($"{preFix} start if loop with offertype payment and direct");
                    var _pymtDirect = campaign.RewardCriteria?.Direct?.Event?.GetPaymentDirect();
                    if (_pymtDirect != null && CheckMerchantAndBiller(_pymtDirect, "Direct"))
                    {
                        continue;
                    }
                }

                if (string.Equals(_offerType, OfferTypeEnum.GENERAL_OFFERS, _strOrd)
                    && string.Equals(_rewardIssuance, RewardIssuanceEnum.WITH_LOCK_UNLOCK, _strOrd)
                    && string.Equals(campaign?.RewardCriteria?.WithLockUnlock?.LockEvent.EventName, EventEnum.SPEND, _strOrd))
                {
                    _logger.LogInformation($"{preFix} start if loop with offertype payment and lock");
                    var _pymtLock = campaign.RewardCriteria?.WithLockUnlock?.LockEvent?.Event?.GetPaymentDirect();
                    if (_pymtLock != null && CheckMerchantAndBiller(_pymtLock, "Lock"))
                    {
                        continue;
                    }
                }

                if (string.Equals(_offerType, OfferTypeEnum.GENERAL_OFFERS, _strOrd)
                    && string.Equals(_rewardIssuance, RewardIssuanceEnum.WITH_LOCK_UNLOCK, _strOrd)
                    && string.Equals(campaign?.RewardCriteria?.WithLockUnlock?.UnlockEvent.EventName, EventEnum.SPEND, _strOrd))
                {
                    _logger.LogInformation($"{preFix} start if loop with offertype payment and unlock");
                    var _pymtUnlock = campaign.RewardCriteria?.WithLockUnlock?.UnlockEvent?.Event?.GetPaymentDirect();
                    if (_pymtUnlock != null && CheckMerchantAndBiller(_pymtUnlock, "Unlock"))
                    {
                        continue;
                    }
                }

                bool CheckMerchantAndBiller(CampaignModel.PaymentDirect _pymtEvent, string rewardIssuance)
                {
                    var isQualified = false;

                    // if campaign is of any type, then add in the qualified campaign and continue
                    if (string.Equals(_pymtEvent?.TransactionType, "Any", _strOrd))
                    {
                        // add this campaign in cumulative campaign and continue 
                        quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                        _logger.LogInformation("Processing end for ::{PreFix} Current Campaign Id :: {campaign}", preFix, campaign.Id);
                        isQualified = true;
                        return isQualified;
                    }

                    var _pymtCats = _pymtEvent?.PaymentCategories;

                    if (!_pymtCats.Contains(_txnPymtCat, StringComparer.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation($"{preFix} Payment Categories dosenot matched : {JsonConvert.SerializeObject(_pymtCats)}");
                        // don't qualify campaign if payment category doesn't match
                        _logger.LogInformation("Processing end for ::{PreFix} Current Campaign Id :: {campaign}", preFix, campaign.Id);
                        return isQualified;
                    }

                    // here we check if any of the transaction paymentinstrument exists in campaign paymentinstrument
                    var _pymtInstrmt = _pymtEvent?.PaymentInstruments;
                    bool _txnPymtInstrmtHasMatch = _pymtInstrmt.Select(s => s).Intersect(_txnPymtInstrmt, StringComparer.OrdinalIgnoreCase).Any();
                    if (!_txnPymtInstrmtHasMatch)
                    {
                        _logger.LogInformation($"{preFix} Payment Instrument dosenot matched : {JsonConvert.SerializeObject(_pymtInstrmt)}");
                        _logger.LogInformation("Processing end for ::{PreFix} Current Campaign Id :: {campaign}", preFix, campaign.Id);
                        return isQualified;
                    }

                    // Now check the paymentcategory for its attribute level validations.
                    // P2P

                    //JD
                    if (string.Equals(_txnPymtCat, "p2p", _strOrd) || _txnPymtCat == "p2mof" || _txnPymtCat == "p2mon" || _txnPymtCat == "P2MOF" || _txnPymtCat == "P2MON" || _txnPymtCat == "Insurance" || _txnPymtCat == "Confirm" || _txnPymtCat == "Partpay" || _txnPymtCat == "Drawdown" || _txnPymtCat == "subPurchase")
                    {

                        var oncePerDestinationVPA = _pymtEvent.OncePerDestinationVPA;
                        var destinationVpaId = transaction?.TransactionRequest?.TransactionDetail?.Customer?.DestinationVPAId;

                        if (oncePerDestinationVPA)
                        {
                            if (!string.IsNullOrEmpty(destinationVpaId))
                            {


                                var request = new EventManagerWorker.Models.MobileCampaignOncePerDestinationRequest()
                                {
                                    CampaignId = campaign.BFLCampaignId,
                                    MobileNumber = transaction.Customer.MobileNumber,
                                    CustomerId = transaction.Customer.MobileNumber,
                                    MerchantId = null,
                                    BillerId = null,
                                    DestinationVPA = transaction?.TransactionRequest?.TransactionDetail?.Customer?.DestinationVPAId,
                                };
                                _logger.LogInformation($"{preFix} Request for destination VPA: {JsonConvert.SerializeObject(request)}");
                                var result = _oncePerDestionVpaService.GetMobileCampaignDestination(request);
                                _logger.LogInformation($" {preFix} MobileCampaignOncePerDestination_GetMobileCampaignDestinationAsync result {JsonConvert.SerializeObject(result)}");

                                if (!string.IsNullOrEmpty(result?.Id))
                                {
                                    _logger.LogInformation($"{preFix} No unique rewarding for this combination of DestinationVPAId for P2P. Already rewarded for this.");
                                    return isQualified;
                                }
                            }
                            else
                            {
                                _logger.LogInformation($"{preFix} Destination VPA Id is empty no rewarding.");
                                return isQualified;
                            }
                        }
                        //JD VPA
                        bool IsVapSegmentCampaignQualify = false;
                        //JD VPA
                        if (string.Equals(_txnPymtCat, "P2MOF", _strOrd) || string.Equals(_txnPymtCat, "p2mof", _strOrd))
                        {
                            IsVapSegmentCampaignQualify = _processorService.ApplyVPASegmentFilter(campaign, _txnDetail, "P2MOF", "Payment", preFix);
                        }
                        else if (string.Equals(_txnPymtCat, "P2MON", _strOrd) || string.Equals(_txnPymtCat, "p2mon", _strOrd))
                        {
                            IsVapSegmentCampaignQualify = _processorService.ApplyVPASegmentFilter(campaign, _txnDetail, "P2MON", "Payment", preFix);
                        }
                        else
                        {
                            IsVapSegmentCampaignQualify = true;
                        }

                        _logger.LogInformation($"{preFix} :::: _txnPymtCat:{_txnPymtCat} IsVapSegmentCampaignQualify: {IsVapSegmentCampaignQualify}");
                        //JD VPA
                        if (!IsVapSegmentCampaignQualify)
                        {
                            _logger.LogInformation($"{preFix} Campaign is not quality as it not match with the vpa segment campaign.Id : {campaign.Id}");
                            return isQualified;
                        }

                        if (string.Equals(_pymtEvent.TransactionType, "Cumulative", _strOrd))
                        {
                            // add this campaign in cumulative campaign and continue 
                            quaifiedCumulativeCampaigns = quaifiedCumulativeCampaigns.Append(campaign);
                            _logger.LogInformation("Processing end for ::{PreFix} Current Campaign Id :: {campaign}", preFix, campaign.Id);
                            isQualified = true;
                            return isQualified;
                        }



                        var _txnamt = _txnDetail.Amount;
                        var _mintxnamt = _pymtEvent.Single?.MinTransactionAmount;
                        var _ismintxnamt = (bool)_pymtEvent.Single?.IsTransactionAmount;

                        _logger.LogInformation($"{preFix} _txnamt:{_txnamt}");
                        _logger.LogInformation($"{preFix} _mintxnamt:{_mintxnamt}");
                        _logger.LogInformation($"{preFix} _ismintxnamt:{_ismintxnamt}");
                        //JD VPA
                        if (!_ismintxnamt || _txnamt >= Convert.ToDouble(_mintxnamt) && IsVapSegmentCampaignQualify)
                        {
                            quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                            _logger.LogInformation($"Processing end for ::{preFix} Current Campaign Id :: {campaign.Id}");
                            isQualified = true;
                            return isQualified;
                        }
                    }


                    // P2M
                    if (string.Equals(_txnPymtCat, "p2m", _strOrd))
                    {
                        var _cmpgnMchntRDlr = _pymtEvent.Merchant;

                        var oncePerDestinationVPA = _pymtEvent.OncePerDestinationVPA;
                        var merchantId = transaction?.TransactionRequest?.TransactionDetail?.MerchantOrDealer?.Id;

                        if (oncePerDestinationVPA)
                        {
                            if (!string.IsNullOrEmpty(merchantId))
                            {
                                var request = new EventManagerWorker.Models.MobileCampaignOncePerDestinationRequest()
                                {
                                    CampaignId = campaign.BFLCampaignId,
                                    MobileNumber = transaction.Customer.MobileNumber,
                                    CustomerId = transaction.Customer.MobileNumber,
                                    MerchantId = merchantId,
                                    BillerId = null,
                                    DestinationVPA = null
                                };
                                _logger.LogInformation($"Request for destination VPA: {JsonConvert.SerializeObject(request)}");
                                var result = _oncePerDestionVpaService.GetMobileCampaignDestination(request);
                                _logger.LogInformation($" {preFix} MobileCampaignOncePerDestination_GetMobileCampaignDestinationAsync result {JsonConvert.SerializeObject(result)}");

                                if (!string.IsNullOrEmpty(result?.Id))
                                {
                                    _logger.LogInformation($"{preFix} No unique rewarding for this combination of MerchantId for P2M. Already rewarded for this.");
                                    return isQualified;
                                }
                            }
                            else
                            {
                                _logger.LogInformation($"{preFix} No unique rewarding for this MerchantId for P2M. MerchantId id is empty .");
                                return isQualified;
                            }
                        }

                        _logger.LogInformation($"Campaign MerchantBiller : {JsonConvert.SerializeObject(_cmpgnMchntRDlr)}");

                        // Add in qualifiedTripleRewardCampaigns
                        // TODO : need to discuss for the change Remove for crossLob function bfl-2653
                        //if (string.Equals(transaction.TransactionRequest.EventId, "Spend", _strOrd) && string.Equals(transaction.TransactionRequest.LOB, "H45-REMUPI", _strOrd))
                        if (string.Equals(transaction.TransactionRequest.EventId, "Spend", _strOrd) && string.Equals(campaign.OfferType, OfferTypeEnum.TRIPLE_REWARD, _strOrd))
                        {
                            string merchantType = _pymtEvent.Merchant.MerchantType;
                            // var tripleRewardMerchants = _webUIDatabaseService.GetMerchantDBEnumValues(_txnDetail.MerchantOrDealer.Category, _txnDetail.MerchantOrDealer.GroupId, _txnDetail.MerchantOrDealer.Id, _txnDetail.MerchantOrDealer.Source, 1, merchantType).GetAwaiter().GetResult();
                            var tripleRewardMerchants = _merchantMasterService.GetMerchantMasterValues(new MerchantEnumRequest()
                            {
                                Category = _txnDetail.MerchantOrDealer.Category,
                                GroupMerchantId = _txnDetail.MerchantOrDealer.GroupId,
                                MerchantId = _txnDetail.MerchantOrDealer.Id,
                                Source = _txnDetail.MerchantOrDealer.Source,
                                TripleReward = 1,
                                MerchantType = merchantType
                            });

                            if (tripleRewardMerchants == null)// || !tripleRewardMerchants.Any())
                            {
                                qualifiedTripleRewardCampaigns = qualifiedTripleRewardCampaigns.Append(campaign);
                                isQualified = true;
                                return isQualified;
                            }
                            else
                            {
                                var responseFlag = _processorService.ValidateMerchantSegment(_cmpgnMchntRDlr, _txnDetail);
                                if (responseFlag)
                                {
                                    qualifiedTripleRewardCampaigns = qualifiedTripleRewardCampaigns.Append(campaign);
                                    isQualified = true;
                                    return isQualified;
                                }
                            }
                        }

                        // This Code is Done For Any Merchant Check.

                        // If Campaign Has Configured For Any Merchant. Then Check MerchantSegment == "Any". If Condition Pass Add into Qualified Campaign List.
                        if (string.Equals(_cmpgnMchntRDlr.MerchantSegment, "Any", _strOrd))
                        {

                            if (string.Equals(_pymtEvent.TransactionType, "Cumulative", _strOrd))
                            {
                                _logger.LogInformation($"{preFix} Adding To Cumulative Qualified Campaign.");
                                // add this campaign in cumulative campaign and continue 
                                quaifiedCumulativeCampaigns = quaifiedCumulativeCampaigns.Append(campaign);
                                _logger.LogInformation("Processing end for ::{PreFix} Current Campaign Id :: {campaign}", preFix, campaign.Id);
                                isQualified = true;
                                return isQualified;
                            }
                            var responseFlag = _processorService.ValidateMerchantSegment(_cmpgnMchntRDlr, _txnDetail);
                            _logger.LogInformation($"{preFix} in any case ValidateMerchantSegment value" + responseFlag);

                            if (responseFlag)
                            {
                                _logger.LogInformation($"{preFix} Adding To Qualified Campaign.");
                                quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                                isQualified = true;
                                return isQualified;
                            }
                            _logger.LogInformation("Processing end for ::{PreFix} Current Campaign Id :: {campaign}", preFix, campaign.Id);
                        }
                        else
                        {
                            var responseFlag = _processorService.ValidateMerchantSegment(_cmpgnMchntRDlr, _txnDetail);
                            _logger.LogInformation($"in other case ValidateMerchantSegment value" + responseFlag);

                            if (responseFlag)
                            {
                                quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                                _logger.LogInformation("Processing end for _cmpgnMchntRDlr.MerchantSegment, Any ::{PreFix} Current Campaign Id :: {campaign}", preFix, campaign.Id);
                                isQualified = true;
                                return isQualified;
                            }
                        }
                    }

                    // Check for BBPS
                    if (string.Equals(_txnPymtCat, "bbps", _strOrd))
                    {
                        var oncePerDestinationVPA = _pymtEvent.OncePerDestinationVPA;
                        var billerId = transaction?.TransactionRequest?.TransactionDetail?.Biller?.Id;

                        var paymentDetail = _pymtEvent;
                        bool isBillerCategoryMatched = false;

                        if (oncePerDestinationVPA)
                        {
                            if (!string.IsNullOrEmpty(billerId))
                            {
                                var request = new EventManagerWorker.Models.MobileCampaignOncePerDestinationRequest()
                                {
                                    CampaignId = campaign.BFLCampaignId,
                                    MobileNumber = transaction.Customer.MobileNumber,
                                    CustomerId = transaction.Customer.MobileNumber,
                                    MerchantId = null,
                                    BillerId = billerId,
                                    DestinationVPA = null
                                };
                                _logger.LogInformation($"{preFix} Request for Unique P2P for BBPS : {JsonConvert.SerializeObject(request)}");
                                var result = _oncePerDestionVpaService.GetMobileCampaignDestination(request);
                                _logger.LogInformation($" {preFix} MobileCampaignOncePerDestination_GetMobileCampaignDestinationAsync result {JsonConvert.SerializeObject(result)}");

                                if (!string.IsNullOrEmpty(result?.Id))
                                {
                                    _logger.LogInformation($"{preFix} No unique rewarding for this combination of BillerId for BBPS. Already rewarded for this.");
                                    return isQualified;
                                }
                            }
                            else
                            {
                                _logger.LogInformation($"{preFix} No unique rewarding BillerId is Null or empty.");
                                return isQualified;
                            }
                        }

                        if (paymentDetail.BBPS.AnyCategory)
                        {
                            isBillerCategoryMatched = true;
                        }
                        else
                        {
                            var _cmpgnBllrCats = paymentDetail.BBPS.BillerCategories;
                            var _txnBllrCat = _txnDetail.Biller;

                            foreach (var campaignBillerCategory in _cmpgnBllrCats)
                            {
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

                        if (isBillerCategoryMatched)
                        {
                            if (string.Equals(paymentDetail.TransactionType, "Cumulative", _strOrd))
                            {
                                _logger.LogInformation($"{preFix} Adding To Cumulative Qualified Campaign.");
                                // add this campaign in cumulative campaign and continue 
                                quaifiedCumulativeCampaigns = quaifiedCumulativeCampaigns.Append(campaign);
                                _logger.LogInformation("Processing end for ::{PreFix} Current Campaign Id :: {campaign}", preFix, campaign.Id);
                                isQualified = true;
                                return isQualified;
                            }
                            _logger.LogInformation($"{preFix} Adding To Qualified Campaign.");
                            quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                            _logger.LogInformation("Processing end for ::{PreFix} Current Campaign Id :: {campaign}", preFix, campaign.Id);
                            isQualified = true;
                            return isQualified;
                        }
                    }
                    return isQualified;
                }

                _logger.LogInformation("Processing end for ::{PreFix} Current Campaign Id :: {campaign}", preFix, campaign.Id);
            }

            //_logger.LogInformation("{PreFix} Campaigns After Processing => Qualified :  {campaigns}", preFix, quaifiedCampaigns == null ? null : JsonConvert.SerializeObject(quaifiedCampaigns));
            _logger.LogInformation("{PreFix} Campaigns After Processing => Qualified :  {campaigns}", preFix, quaifiedCampaigns == null ? null : string.Join(",", campaigns.Select(c => c.Id)));
            //_logger.LogInformation("{PreFix} Campaigns After Processing => QuaifiedCumulativeCampaigns :  {campaigns}", preFix, quaifiedCumulativeCampaigns == null ? null : JsonConvert.SerializeObject(quaifiedCumulativeCampaigns));
            _logger.LogInformation("{PreFix} Campaigns After Processing => QuaifiedCumulativeCampaigns :  {campaigns}", preFix, quaifiedCumulativeCampaigns == null ? null : string.Join(",", campaigns.Select(c => c.Id)));
            //_logger.LogInformation("{PreFix} Campaigns After Processing => QualifiedTripleRewardCampaigns :  {campaigns}", preFix, qualifiedTripleRewardCampaigns == null ? null : JsonConvert.SerializeObject(qualifiedTripleRewardCampaigns));
            _logger.LogInformation("{PreFix} Campaigns After Processing => QualifiedTripleRewardCampaigns :  {campaigns}", preFix, qualifiedTripleRewardCampaigns == null ? null : string.Join(",", campaigns.Select(c => c.Id)));

            _logger.LogInformation("===================== {PreFix} ApplyMerchantAndBillerFilter Processing End =============================", preFix);
            _logger.LogInformation("============================================================================================================");
            return (quaifiedCampaigns, quaifiedCumulativeCampaigns, qualifiedTripleRewardCampaigns);
            // change parameter to return two object
            // return quaifiedCumulativeCampaigns;
        }
        // Not Move In Extension Need Refactor
        private List<CampaignModel.EarnCampaign> GetCampaigns(TransactionModel.ProcessedTransaction transaction)
        {
            var basicQuery = transaction.PrepareFilterQueryWithCollection();
            _logger.LogInformation($" {preFix} GetCampaigns Applied Filter : {JsonConvert.SerializeObject(basicQuery)}");

            var filteredCampaigns = GetCampaignByApplyFilter(basicQuery);
            return filteredCampaigns;
        }

        private List<CampaignModel.EarnCampaign> GetCampaignByApplyFilter(FilterDefinition<CampaignModel.EarnCampaign> filter) => _processorService.GetCampaignsUsingFilter(filter);

        private TransactionModel.ProcessedTransaction ProcessCumulativeTransactionLogs(TransactionModel.ProcessedTransaction transaction)
        {
            return _processorService.InsertIntoCumulativeTransactionLogs(transaction);
        }

        private IEnumerable<CampaignModel.EarnCampaign> ApplyTripleRewardMerchantFilter(TransactionModel.ProcessedTransaction processedTransaction, IEnumerable<CampaignModel.EarnCampaign> tripleRewardCampaigns)
        {
            IEnumerable<CampaignModel.EarnCampaign> qualifiedTripleRewardCampaigns = new List<CampaignModel.EarnCampaign>();
            //var transactionMerchant = processedTransaction.TransactionRequest.TransactionDetail.MerchantOrDealer;
            //var tripleRewardMerchant = _processorService.GetTripleRewardMerchant(transactionMerchant.Category, transactionMerchant.GroupId, transactionMerchant.Id);
            //if (tripleRewardMerchant == null || !tripleRewardMerchant.Any())
            //{
            //    _logger.LogInformation($"Triple Reward Merchant Not Found for Category : {transactionMerchant.Category}, GroupId : { transactionMerchant.GroupId }, Id : {transactionMerchant.Id}");
            //    return qualifiedTripleRewardCampaigns;
            //}

            foreach (var tripleRewardCampaign in tripleRewardCampaigns)
            {
                if (tripleRewardCampaign.Filter.IsTripleReward)
                {

                    var asScratchCard = tripleRewardCampaign.RewardCriteria.AsScratchCard;

                    if (!(asScratchCard.TransactionType == "single"))
                    {
                        _logger.LogInformation("Triple Reward Campaign is not Single");
                        continue;
                    }
                    if (!(asScratchCard.Duration.Recurrence == "daily"))
                    {
                        _logger.LogInformation("Triple Reward Campaign is not Configured as Daily.");
                        continue;
                    }
                    //Transaction Amount Check with configuration
                    if (asScratchCard.Duration.IsMinTransaction)
                    {
                        if (!(processedTransaction.TransactionRequest.TransactionDetail.Amount >= asScratchCard.Duration.MinTransactionAmount))
                        {
                            _logger.LogInformation("Transaction Amount is not meet the minimum amount check.");
                            continue;
                        }
                    }
                    var rewardedTransaction = GetRewardedTransactions(processedTransaction, tripleRewardCampaign).GetAwaiter().GetResult();

                    if (rewardedTransaction.Count > 0)
                    {
                        _logger.LogInformation($"Customer: {rewardedTransaction.FirstOrDefault().MobileNumber} is already rewarded in Campaign: {rewardedTransaction.FirstOrDefault().CampaignId}");
                        continue;
                    }
                    qualifiedTripleRewardCampaigns = qualifiedTripleRewardCampaigns.Append(tripleRewardCampaign);
                }
            }
            return qualifiedTripleRewardCampaigns;
        }

        private async Task<List<MongoService.TripleRewardCustomerMerchantMapping>> GetRewardedTransactions(TransactionModel.ProcessedTransaction processedTransaction, CampaignModel.EarnCampaign tripleRewardCampaign)
        {
            var rewardCriteria = tripleRewardCampaign.RewardCriteria.AsScratchCard;
            var merchantDealer = processedTransaction.TransactionRequest.TransactionDetail.MerchantOrDealer;
            var tripleRewardMerchantMappingRequest = new MongoService.TripleRewardCustomerMerchantMapping()
            {
                MerchantCategory = merchantDealer.Category,
                MerchantGroup = merchantDealer.GroupId,
                MerchantId = merchantDealer.Id,
                MobileNumber = processedTransaction.TransactionRequest.MobileNumber,
                CampaignId = tripleRewardCampaign.Id,
                RewardedOn = DateTime.Now
            };
            var tripleRewardMerchantMappings = await _processorService.GetTripleRewardMerchantMappings(tripleRewardMerchantMappingRequest).ConfigureAwait(false);

            var r = tripleRewardMerchantMappings.Where(o => Convert.ToDateTime(o.RewardedOn).Date == DateTime.Now.Date).ToList();

            return r;
        }

        //private bool IsMerchantDealerValid(CampaignModel.Merchant currentCampaignMerchantDealer, Models.Common.TransactionModel.MerchantOrDealer p2mTypeCutomerTransactionMerchantDealer)
        //{
        //    return currentCampaignMerchantDealer.MerchantCategory == p2mTypeCutomerTransactionMerchantDealer.Category && currentCampaignMerchantDealer.GroupMerchantId == p2mTypeCutomerTransactionMerchantDealer.GroupId && currentCampaignMerchantDealer.MerchantId == p2mTypeCutomerTransactionMerchantDealer.Id;
        //}
        private TransactionModel.ProcessedTransaction SortCumulativeCampaigns(TransactionModel.ProcessedTransaction transaction, List<CampaignModel.EarnCampaign> cumulativeCampaigns)
        {
            List<TransactionModel.MatchedCampaign> matchedCampaigns = new List<TransactionModel.MatchedCampaign>();
            foreach (var campaign in cumulativeCampaigns)
            {
                var isReferral = campaign.Filter?.IsRefferalProgram;
                var matchedCampaign = new TransactionModel.MatchedCampaign()
                {
                    CampaignId = campaign.Id,
                    EventType = transaction.TransactionRequest.EventId.GetEventCodeByEventName(),
                    RewardCriteria = campaign.RewardCriteria,
                    RewardOptions = campaign.Filter.IsMembershipReward ? _processorService.GetMembershipRewardOptions(campaign) : _processorService.GetRewardOptions(campaign),
                    IsDirect = campaign.IsDirectCampaign(),
                    IsUnLock = campaign.IsUnlockCampaign(transaction).IsUnlock,
                    IsLock = campaign.IsLockCampaign(transaction),
                    StartDate = campaign.StartDate,
                    EndDate = campaign.EndDate,
                    OfferType = campaign.OfferType,
                    Narration = campaign.Filter.IsLock ? campaign.Content.UnlockCondition : campaign.Content.RewardNarration,
                    IsOncePerCampaign = campaign.OncePerCampaign,
                    OnceInLifeTime = campaign.OnceInLifeTime,
                    CTAUrl = campaign.Content.CTAUrl,  //chetan
                    UnlockTermAndCondition = campaign.Content.UnlockTermAndCondition,  //chetan
                    IsReferralProgram = campaign.Filter.IsRefferalProgram,
                    ReferralRewardOptions = isReferral == true ? _processorService.GetReferralRewardOptions(campaign) : null,
                    IsAssuredCashbackKickOff = campaign.Filter.IsAssuredCashbackKickOff,
                    GenericLockCard = campaign.Content.GenericLockCard,
                    ReferralLockCard = campaign.Content.GenericLockCard
                };
                matchedCampaigns.Add(matchedCampaign);
            }
            transaction.MatchedCampaigns = matchedCampaigns;
            return transaction;
        }
        private IEnumerable<CampaignModel.EarnCampaign> ApplyRMSFilter(IEnumerable<CampaignModel.EarnCampaign> campaigns, CustomerModel.Customer customer, CustomerModel.CustomerSummary customerSummary, CustomerModel.Customer subCustomer)
        {
            IEnumerable<CampaignModel.EarnCampaign> quaifiedCampaigns = new List<CampaignModel.EarnCampaign>();
            _logger.LogInformation("=========================================================================================================================");
            _logger.LogInformation("================= {PreFix} Campaigns Before ApplyRMSFilter {campaigns}===============", preFix, campaigns == null ? null : JsonConvert.SerializeObject(campaigns));
            foreach (var campaign in campaigns)
            {
                // if rmsattribute doesn't exists in campaign i.e., 
                // its null or empty then
                // qualify the campaign
                // else
                // check if customer has earned some rewards (customer summary is not null)
                // then check if the customer has earned according to all rms attributes (points, cashback, subscription)
                // with min and max points

                if (campaign.RmsAttributes == null || campaign.RmsAttributes?.Count <= 0)
                {
                    quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                }
                else
                {
                    // if customer summary exists and rmsattribute in campaign exits
                    if (customerSummary != null || subCustomer != null)
                    {
                        const StringComparison _strOrd = StringComparison.OrdinalIgnoreCase;
                        var flag = true;
                        foreach (var attribute in campaign.RmsAttributes)
                        {
                            var pc = attribute.parameterCode;
                            var _s = attribute.StartRange;
                            var _e = attribute.EndRange;

                            if (string.Equals(attribute.AttributeType, "Points", _strOrd) && customerSummary.Point != null)
                            {
                                var _cspoint = customerSummary.Point;

                                var _f = false;
                                double? _cspnt = 0.0;
                                switch (pc)
                                {
                                    case string p when pc.Equals("LifeTimeEarn", _strOrd):
                                        _f = true;
                                        _cspnt = _cspoint?.LifeTimeEarn;
                                        break;
                                    case string p when pc.Equals("LifeTimeExpired", _strOrd):
                                        _f = true;
                                        _cspnt = _cspoint?.LifeTimeExpired;
                                        break;
                                    case string p when pc.Equals("LifeTimeRedeemed", _strOrd):
                                        _f = true;
                                        _cspnt = _cspoint?.LifeTimeRedeemed;
                                        break;
                                    case string p when pc.Equals("AvailableBalance", _strOrd):
                                        _f = true;
                                        _cspnt = _cspoint?.AvailableBalance;
                                        break;
                                    case string p when pc.Equals("CurrentMonthEarned", _strOrd):
                                        _f = true;
                                        _cspnt = _cspoint?.CurrentMonthEarned;
                                        break;
                                    case string p when pc.Equals("CurrentMonthRedeemed", _strOrd):
                                        _f = true;
                                        _cspnt = _cspoint?.CurrentMonthRedeemed;
                                        break;
                                    case string p when pc.Equals("CurrentMonthExpired", _strOrd):
                                        _f = true;
                                        _cspnt = _cspoint?.CurrentMonthExpired;
                                        break;
                                    default:
                                        // log that no rmsattribute matched.
                                        _cspnt = 0.0;
                                        break;
                                }

                                if (_f && !(_cspnt >= _s && _cspnt <= _e))
                                {
                                    flag = false;
                                }
                            }

                            if (customerSummary.Cashback != null && string.Equals(attribute.AttributeType, "Cashback", _strOrd))
                            {
                                var _cscashback = customerSummary.Cashback;

                                var _f = false;
                                double? _cspnt = 0.0;
                                switch (pc)
                                {
                                    case string p when pc.Equals("LifeTimeEarn", _strOrd):
                                        _f = true;
                                        _cspnt = _cscashback?.LifeTimeEarn;
                                        break;
                                    case string p when pc.Equals("CurrentMonthEarned", _strOrd):
                                        _f = true;
                                        _cspnt = _cscashback?.CurrentMonthEarned;
                                        break;
                                    default:
                                        // log that no rmsattribute matched.
                                        _cspnt = 0.0;
                                        break;
                                }
                                if (_f && !(_cspnt >= _s && _cspnt <= _e))
                                {
                                    flag = false;
                                }
                            }

                            if (string.Equals(attribute.AttributeType, "Subscription", _strOrd))
                            {
                                var customerSubType = subCustomer.SubscriptionType ?? string.Empty;
                                int customerTierId = subCustomer.SubscriptionTier != null ? subCustomer.SubscriptionTier.TierId : 0;

                                if (string.Equals(customerSubType, "Paid", _strOrd))
                                {
                                    bool tierMatch = attribute.SubscriptionTiers != null &&
                                                 attribute.SubscriptionTiers.Any(t => t.TierId == customerTierId);

                                    if (!tierMatch)
                                    {
                                        flag = false;
                                        break;
                                    }
                                }
                            }
                        }

                        if (flag)
                        {
                            quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                        }
                    }
                }
            }
            _logger.LogInformation("================= {PreFix} Campaigns After ApplyRMSFilter {campaigns}===============", preFix, campaigns == null ? null : JsonConvert.SerializeObject(campaigns));
            _logger.LogInformation("=========================================================================================================================");
            return quaifiedCampaigns;
        }
        private IEnumerable<CampaignModel.EarnCampaign> ApplyCustomerStatusFilter(IEnumerable<CampaignModel.EarnCampaign> earnCampaigns, TransactionModel.ProcessedTransaction processedTransaction/*, List<DBEnumValue> enumValues*/)
        {
            IEnumerable<CampaignModel.EarnCampaign> qualifiedCampaigns = new List<CampaignModel.EarnCampaign>();
            var customer = processedTransaction.Customer;
            _logger.LogInformation("=========================================================================================================================");
            _logger.LogInformation("================{PreFix} Campaign Before ApplyCustomerStatusFilter {campaigns}================", preFix, earnCampaigns == null ? null : JsonConvert.SerializeObject(earnCampaigns));

            _logger.LogInformation("{PreFix} Customer {customer}", preFix, JsonConvert.SerializeObject(customer));

            foreach (var campaign in earnCampaigns)
            {
                var campaignCustomerStatus = campaign.CustomerStatus;

                if ((customer.Flags == null) || (!customer.Flags.GlobalDeliquient && !customer.Flags.Dormant && (customer.Flags.LobFraud == null || !customer.Flags.LobFraud.Any()) && (customer.Flags.LoyaltyFraud == 0)))
                {
                    qualifiedCampaigns = qualifiedCampaigns.Append(campaign);
                    continue;
                }
                if (customer.Flags.LoyaltyFraud == 2)
                {
                    continue;
                }

                List<string> customerFlags = new List<string>();

                #region Selection Area
                if (customer.Flags.GlobalDeliquient)
                {
                    customerFlags.Add("GDL");
                }
                if (customer.Flags.Dormant)
                {
                    customerFlags.Add("DRM");
                }
                var configuredSubscriptionReward = campaign.RewardOption.Where(o => String.Equals(o.RewardType, "Subscription", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (configuredSubscriptionReward != null)
                {
                    if (String.IsNullOrEmpty(configuredSubscriptionReward.SubscriptionDetails.SubscriptionId))
                    {
                        continue;
                    }
                    if (!(customer.Flags.LoyaltyFraud == 0))
                    {
                        continue;
                    }
                    if (customer.Flags.LobFraud != null && customer.Flags.LobFraud.Any())
                    {
                        var subscription = _processorService.GetSubscriptionById(preFix, configuredSubscriptionReward.SubscriptionDetails.SubscriptionId);
                        if (subscription == null)
                        {
                            continue;
                        }
                        if (customer.Flags.LobFraud.Contains(subscription.Lob))
                        {
                            continue;
                        }
                    }
                }
                if (customer.Flags.LoyaltyFraud == 1)
                {
                    customerFlags.Add("RMS");
                }
                if (customer.Flags.LobFraud != null && customer.Flags.LobFraud.Any())
                {
                    customerFlags.Add("LOB");
                }
                #endregion

                var counter = 0;

                if (customerFlags.Any())
                {
                    //if (campaignCustomerStatus != null && campaignCustomerStatus.Count > 0)
                    //{
                    foreach (var customerFlag in customerFlags)
                    {
                        var flag = ValidateCustomerStatus(customerFlag, campaignCustomerStatus, campaign, customer);
                        if (flag)
                        {
                            counter++;
                        }
                    }
                    //}
                }

                if (counter == customerFlags.Count)
                {
                    qualifiedCampaigns = qualifiedCampaigns.Append(campaign);
                }
            }
            _logger.LogInformation("================{PreFix} Campaign After ApplyCustomerStatusFilter {campaigns}================", preFix, qualifiedCampaigns == null ? null : JsonConvert.SerializeObject(qualifiedCampaigns));
            _logger.LogInformation("=========================================================================================================================");
            return qualifiedCampaigns;
        }

        private IEnumerable<CampaignModel.EarnCampaign> ApplyReferrerCustomerStatusFilter(IEnumerable<CampaignModel.EarnCampaign> earnCampaigns, TransactionModel.ProcessedTransaction processedTransaction/*, List<DBEnumValue> enumValues*/)
        {
            IEnumerable<CampaignModel.EarnCampaign> qualifiedCampaigns = new List<CampaignModel.EarnCampaign>();
            var customer = processedTransaction.ReferrerCustomer;
            _logger.LogInformation("=========================================================================================================================");
            _logger.LogInformation("================{PreFix} Campaign Before ApplyReferrerCustomerStatusFilter {campaigns}================", preFix, earnCampaigns == null ? null : JsonConvert.SerializeObject(earnCampaigns));

            _logger.LogInformation("{PreFix} Referrer Customer {customer}", preFix, JsonConvert.SerializeObject(customer));

            foreach (var campaign in earnCampaigns)
            {
                var campaignCustomerStatus = campaign.CustomerStatus;

                if ((customer.Flags == null) || (!customer.Flags.GlobalDeliquient && !customer.Flags.Dormant && (customer.Flags.LobFraud == null || !customer.Flags.LobFraud.Any()) && (customer.Flags.LoyaltyFraud == 0)))
                {
                    qualifiedCampaigns = qualifiedCampaigns.Append(campaign);
                    continue;
                }
                if (customer.Flags.LoyaltyFraud == 2)
                {
                    continue;
                }

                List<string> customerFlags = new List<string>();

                #region Selection Area
                if (customer.Flags.GlobalDeliquient)
                {
                    customerFlags.Add("GDL");
                }
                if (customer.Flags.Dormant)
                {
                    customerFlags.Add("DRM");
                }
                var configuredSubscriptionReward = campaign.RewardOption.Where(o => String.Equals(o.RewardType, "Subscription", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (configuredSubscriptionReward != null)
                {
                    if (String.IsNullOrEmpty(configuredSubscriptionReward.SubscriptionDetails.SubscriptionId))
                    {
                        continue;
                    }
                    if (!(customer.Flags.LoyaltyFraud == 0))
                    {
                        continue;
                    }
                    if (customer.Flags.LobFraud != null && customer.Flags.LobFraud.Any())
                    {
                        var subscription = _processorService.GetSubscriptionById(preFix, configuredSubscriptionReward.SubscriptionDetails.SubscriptionId);
                        if (subscription == null)
                        {
                            continue;
                        }
                        if (customer.Flags.LobFraud.Contains(subscription.Lob))
                        {
                            continue;
                        }
                    }
                }
                if (customer.Flags.LoyaltyFraud == 1)
                {
                    customerFlags.Add("RMS");
                }
                if (customer.Flags.LobFraud != null && customer.Flags.LobFraud.Any())
                {
                    customerFlags.Add("LOB");
                }
                #endregion

                var counter = 0;

                if (customerFlags.Any())
                {
                    //if (campaignCustomerStatus != null && campaignCustomerStatus.Count > 0)
                    //{
                    foreach (var customerFlag in customerFlags)
                    {
                        var flag = ValidateCustomerStatus(customerFlag, campaignCustomerStatus, campaign, customer);
                        if (flag)
                        {
                            counter++;
                        }
                    }
                    //}
                }

                if (counter == customerFlags.Count)
                {
                    qualifiedCampaigns = qualifiedCampaigns.Append(campaign);
                }
            }
            _logger.LogInformation("================{PreFix} Campaign After ApplyReferrerCustomerStatusFilter {campaigns}================", preFix, qualifiedCampaigns == null ? null : JsonConvert.SerializeObject(qualifiedCampaigns));
            _logger.LogInformation("=========================================================================================================================");
            return qualifiedCampaigns;
        }

        private bool ValidateCustomerStatus(string customerFlag, List<string> campaignCustomerStatus, CampaignModel.EarnCampaign campaign, CustomerModel.Customer customer)
        {
            var flag = false;
            _logger.LogInformation("{PreFix} CampaignCustomerStatus {CampaignCustomerStatus}", preFix, campaignCustomerStatus == null ? null : JsonConvert.SerializeObject(campaignCustomerStatus));
            if (customerFlag == "GDL")
            {
                _logger.LogInformation("{PreFix} Checking  Global Delinquency", preFix);
                if (campaignCustomerStatus != null && campaignCustomerStatus.Count > 0 && campaignCustomerStatus.Contains("Delinquency"))
                {
                    flag = true;
                    //_logger.LogInformation("{PreFix} Global Delinquency {flag} PASSED.", preFix, flag);
                }
            }
            else if (customerFlag == "DRM")
            {
                _logger.LogInformation("{PreFix} Checking  Dormant", preFix);
                if (campaignCustomerStatus != null && campaignCustomerStatus.Count > 0 && campaignCustomerStatus.Contains("Dormant"))
                {
                    flag = true;
                    //_logger.LogInformation("{PreFix} Dormant {flag} PASSED.", preFix, flag);
                }
            }
            else if (customerFlag == "RMS")
            {
                _logger.LogInformation("{PreFix} Checking  RMS_Loyalty_Fraud", preFix);
                if (campaignCustomerStatus != null && campaignCustomerStatus.Count > 0 && campaignCustomerStatus.Contains("RMS_Loyalty_Fraud"))
                {
                    flag = true;
                }
            }
            else if (customerFlag == "LOB")
            {
                _logger.LogInformation("{PreFix} Checking  LOB_Fraud", preFix);
                if (customer.Flags.LobFraud.Contains(campaign.LOB))
                {
                    _logger.LogInformation("{PreFix} Checking  Customer.Flags.LobFraud", preFix);
                    if (campaignCustomerStatus != null && campaignCustomerStatus.Count > 0 && campaignCustomerStatus.Contains("LOB_Fraud"))
                    {
                        flag = true;
                    }
                }
                else
                {
                    flag = true;
                }

            }
            _logger.LogInformation("{PreFix} ::: {flag}", preFix, flag);
            return flag;
        }
        private IEnumerable<CampaignModel.EarnCampaign> ApplyInstallationSourceFilter(IEnumerable<CampaignModel.EarnCampaign> earnCampaigns, CustomerModel.Customer customer)
        {
            IEnumerable<CampaignModel.EarnCampaign> qualifiedCampaigns = new List<CampaignModel.EarnCampaign>();
            _logger.LogInformation("=========================================================================================================================");
            _logger.LogInformation("================{PreFix} Campaign Before ApplyInstallationSourceFilter {campaigns}================", preFix, earnCampaigns == null ? null : JsonConvert.SerializeObject(earnCampaigns));

            _logger.LogInformation("{PreFix} Customer Installation Source : {customerInstallationSource}", preFix, customer.Install?.Source);
            foreach (var campaign in earnCampaigns)
            {
                _logger.LogInformation("{PreFix} Campaign Installation Source : {campaignInstallationSource}", preFix, campaign.InstallationSource == null ? null : JsonConvert.SerializeObject(campaign.InstallationSource));
                if ((campaign.InstallationSource?.Source == null) || campaign.InstallationSource.AnyInstallationSource)
                {
                    qualifiedCampaigns = qualifiedCampaigns.Append(campaign);
                    continue;
                }
                if (!String.IsNullOrEmpty(customer.Install.Source))
                {
                    var isCustomerInstallationSourceMatched = campaign.InstallationSource.Source.Contains(customer.Install.Source);
                    if (isCustomerInstallationSourceMatched)
                    {
                        qualifiedCampaigns = qualifiedCampaigns.Append(campaign);
                        continue;
                    }
                }
            }
            _logger.LogInformation("================{PreFix} Campaign after ApplyInstallationSourceFilter {campaigns}================", preFix, qualifiedCampaigns == null ? null : JsonConvert.SerializeObject(qualifiedCampaigns));
            _logger.LogInformation("=========================================================================================================================");
            return qualifiedCampaigns;
        }
        #endregion
        private void PublishInKafka(TransactionModel.ProcessedTransaction transaction) => _messageQueueService.ProduceTransaction(OfferMapTopicName, Guid.NewGuid().ToString(), JsonConvert.SerializeObject(transaction));
        private void PublishInKafka(TransactionModel.ProcessedTransaction transaction, string topicName) => _messageQueueService.ProduceTransaction(topicName, Guid.NewGuid().ToString(), JsonConvert.SerializeObject(transaction));

        private async Task<CustomerModel.Customer> CustomerOnboarding(TransactionModel.ProcessedTransaction transactionReq)
        {
            string prefix = $"TransactionMobileNumber : {transactionReq.Customer.MobileNumber}, TransactionReferenceNumber : {transactionReq.TransactionRequest.TransactionDetail.RefNumber} ::: ";
            _logger.LogInformation($" CustomerOnboarding started and  prefix is : {prefix}");
            var _customer = await _customerService.GetByMobileNumberAsync(transactionReq.Customer.MobileNumber).ConfigureAwait(false);
            CustomerModel.Customer customer = new CustomerModel.Customer()
            {
                MobileNumber = transactionReq.Customer.MobileNumber,

            };
            try
            {
                _logger.LogInformation($" customer response : {JsonConvert.SerializeObject(_customer)} and Mobile number is : {transactionReq.Customer.MobileNumber} and CustomerType is : {transactionReq.TransactionRequest.TransactionDetail.Customer?.CustomerType}");
                string PayloadCustomerType = string.Empty;
                PayloadCustomerType = String.IsNullOrEmpty(transactionReq.TransactionRequest.TransactionDetail.Customer?.CustomerType) ? "NTB" : transactionReq.TransactionRequest.TransactionDetail.Customer?.CustomerType.ToUpper();
                customer.Type = string.IsNullOrEmpty(_customer?.LoyaltyId) ? PayloadCustomerType : _customer.Type;
                if (_customer == null || string.IsNullOrEmpty(_customer?.LoyaltyId))
                {
                    customer.Type = PayloadCustomerType;
                    try
                    {
                        var results = await _eventManagerApiClient.Customers_CreateUpdateCustomerAsync(false, customer.ToCustomerRequestModel()).ConfigureAwait(false);
                        _logger.LogInformation($" results response : {JsonConvert.SerializeObject(results)}");
                        if (results.StatusCode == "2000" || results.StatusCode == "2001")
                        {
                            var retries = Convert.ToInt32(_configuration["Retries"]);
                            var waitTime = TimeSpan.FromMilliseconds(Convert.ToInt32(_configuration["WaitTime"]));
                            _logger.LogInformation($"Retry {retries} and WaitTime: {waitTime}");
                            int i = 0;
                            while (i <= retries)
                            {
                                Task.Delay(waitTime).Wait();
                                var Customer = await _customerService.GetByMobileNumberAsync(transactionReq.Customer.MobileNumber).WaitAsync(waitTime);
                                if (Customer != null)
                                {
                                    customer = Customer;
                                    break;
                                }
                                i++;
                            }
                            //var Customer = await _customerService.GetByMobileNumberAsync(transactionReq.Customer.MobileNumber).ConfigureAwait(false);
                            //customer = Customer;
                            _logger.LogInformation($" GetFinalTransaction started at :{DateTime.Now}");
                            var response = await GetFinalTransaction(transactionReq.TransactionRequest).ConfigureAwait(false);
                            _logger.LogInformation($" GetFinalTransaction response is :{JsonConvert.SerializeObject(response)}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($" {prefix} Error {ex}");
                        throw;
                    }
                }
                else if (!String.IsNullOrEmpty(transactionReq.TransactionRequest.TransactionDetail.Customer?.CustomerType))
                {
                    _logger.LogInformation($" started else block");
                    customer = _customer;
                    _logger.LogInformation($" customer  is : {JsonConvert.SerializeObject(customer)}");
                    int existTypeCustomer = (int)(Enum.Parse<EventManagerWorker.Models.CustomerTypeEnum>(customer.Type));
                    int PayloadTypeCustomer = (int)(Enum.Parse<EventManagerWorker.Models.CustomerTypeEnum>(PayloadCustomerType));
                    _logger.LogInformation($" existTypeCustomer : {existTypeCustomer} and PayloadTypeCustomer is :{PayloadTypeCustomer}");
                    if (PayloadTypeCustomer > existTypeCustomer)
                    {
                        // call update api      
                        customer.Type = PayloadCustomerType;
                        var results = await _eventManagerApiClient.Customers_CreateUpdateCustomerAsync(false, customer.ToCustomerRequestModel()).ConfigureAwait(false);
                        _logger.LogInformation($" results response : {JsonConvert.SerializeObject(results)}");
                        if (results.StatusCode == "2000")
                        {
                            _logger.LogInformation($" GetFinalTransaction started at :{DateTime.Now}");
                            var response = await GetFinalTransaction(transactionReq.TransactionRequest).ConfigureAwait(false);
                            _logger.LogInformation($" GetFinalTransaction response is :{JsonConvert.SerializeObject(response)}");
                        }
                    }
                    else
                    {
                        _logger.LogInformation($" No need to update");
                    }
                }
                else
                {
                    customer = _customer;
                    _logger.LogInformation($" No need to update");
                }
            }
            catch (Exception ex)
            {
                customer.Type = string.Empty;
                _logger.LogError(ex, $" {prefix} Error {ex.Message}");
                throw;
            }
            _logger.LogInformation($" Customer response is  :{JsonConvert.SerializeObject(customer)}");
            return customer;
        }

        private async Task<TransactionModel.ProcessedTransaction> GetFinalTransaction(TransactionModel.Transaction transactions)
        {
            string prefix = $"TransactionMobileNumber : {transactions.MobileNumber}, TransactionReferenceNumber : {transactions.TransactionDetail.RefNumber} ::: ";
            _logger.LogInformation($"{prefix} .......... and  GetFinalTransaction response is :{JsonConvert.SerializeObject(transactions)}");
            try
            {
                CustomerModel.Customer customer = new CustomerModel.Customer();
                customer.Flags = new CommonModel.CustomerModel.Flag() { LoyaltyFraud = 0 };
                customer.MobileNumber = transactions.MobileNumber;
                var _customer = _customerService.GetByMobileNumber(transactions.MobileNumber);
                _logger.LogInformation($"_customer response is :{JsonConvert.SerializeObject(_customer)}");
                if (_customer != null)
                {
                    customer = _customer;
                }
                else
                {
                    return new TransactionModel.ProcessedTransaction() { TransactionRequest = transactions, Customer = customer };
                }
                transactions.CustomerDetail.CustomerVersionId = customer.CustomerVersionId;
                transactions.CustomerDetail.LoyaltyId = customer.LoyaltyId;
                var isUpdateCustomer = false;

                if (string.Equals(transactions.EventId, "WalletCreation", StringComparison.OrdinalIgnoreCase))
                {
                    customer.Flags.Wallet = true;
                    customer.Wallet = SetCustomerWallet();
                    isUpdateCustomer = true;
                }
                else if (string.Equals(transactions.EventId, "VPACreation", StringComparison.OrdinalIgnoreCase))
                {
                    customer.VPA = SetCustomerVPA();
                    isUpdateCustomer = true;
                }
                else if (string.Equals(transactions.EventId, "KYCCompletion", StringComparison.OrdinalIgnoreCase))
                {
                    customer.KYC = SetCustomerKYC(customer.KYC.CompletedDateTime, customer.KYC.Status);
                    isUpdateCustomer = true;
                }
                if (isUpdateCustomer)
                {
                    ProcessCustomerUpdate();
                }
                return new TransactionModel.ProcessedTransaction() { TransactionRequest = transactions, Customer = customer };
                string ProcessCustomerVersion() => _customerVersionService.Create(ModelMapper.Map(customer)).CustomerVersionId;
                void ProcessCustomerUpdate()
                {

                    _logger.LogInformation($" {prefix} :: ProcessCustomerUpdate => TransactionController => ProcessCustomerVersion => Started At {0}", DateTime.Now);
                    customer.CustomerVersionId = ProcessCustomerVersion();
                    _logger.LogInformation($" {prefix} :: ProcessCustomerUpdate => TransactionController => ProcessCustomerVersion => End At {0}", DateTime.Now);
                    _logger.LogInformation($" {prefix} :: FinalCustomerUpdate : {JsonConvert.SerializeObject(customer)}");
                    _logger.LogInformation($" {prefix} :: ProcessCustomerUpdate => TransactionController => CustomerUpdate => Started At {0}", DateTime.Now);
                    _customerService.Update(customer.LoyaltyId, customer);
                    _logger.LogInformation($" {prefix} :: ProcessCustomerUpdate => TransactionController => CustomerUpdate => End At {0}", DateTime.Now);
                }
                CommonModel.Wallet SetCustomerWallet() => new CommonModel.Wallet() { Id = transactions.Wallet.Id, CreatedDateTime = Convert.ToDateTime(transactions.Wallet.CreatedDateTime) };
                CommonModel.VPA SetCustomerVPA()
                {
                    var vpa = transactions.TransactionDetail.Customer.VPA;
                    var newVPA = new CommonModel.VPA()
                    {
                        Id = vpa.Id,
                        CreatedDatetime = Convert.ToDateTime(vpa.CreatedDatetime).ToDomainDateTime(),
                        Status = vpa.Status
                    };
                    _logger.LogInformation($" {prefix} :: SetCustomerVPA : {JsonConvert.SerializeObject(newVPA)}");
                    return newVPA;
                }
                CommonModel.CustomerModel.KYC SetCustomerKYC(DateTime prevDateTime, int currentStatus)
                {
                    var status = transactions.TransactionDetail.Customer.KYCUpgradeFlg;
                    return new CommonModel.CustomerModel.KYC()
                    {
                        Status = status.KYCStatus(),
                        CompletionTag = GetCompletionTag(status, currentStatus),
                        CompletedDateTime = DateTime.Now,
                        PrevDate = prevDateTime
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $" {prefix} Error in GetFinalTransaction : {ex.Message}");
                throw;
            }

        }

        private string GetCompletionTag(string status, int currentStatus)
        {
            try
            {
                if ((currentStatus == 0 || currentStatus == 1) && status == "Full")
                {
                    return "Upgrad";
                }
                else
                {
                    return "Update";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"  Error in GetCompletionTag : {ex.Message}");
                throw;
            }

        }

        private async Task<ResponseOfExternalSegmentList> GetExternalSegmentValidate(ValidateExternalSegmentRequest validateExternalSegmentRequest)
        {
            var response = await _utilServiceClient.Util_ValidateExternalSegmentAsync(validateExternalSegmentRequest).ConfigureAwait(false);
            return response;
        }
    }

}