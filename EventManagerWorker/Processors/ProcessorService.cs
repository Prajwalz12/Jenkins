using Domain.Models;
using Domain.Models.EnumMaster;
using RewardModel = Domain.Models.RewardModel;
using Domain.Services;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using CampaignModel = Domain.Models.CampaignModel;
using CustomerModel = Domain.Models.CustomerModel;
using TransactionModel = Domain.Models.TransactionModel;
using System.Linq;
using EventManagerWorker.Models;
using System.ComponentModel.Design;
using EventManagerWorker.Services.MongoServices.ReferralTable;
using TempReward = Domain.Models.RewardModel.TempReward;
using Domain.Models.TransactionModel;
using Extensions;
using System;
using Domain.Models.CampaignModel;
using Newtonsoft.Json;
using Domain.Models.CustomerModel;
using EventManagerWorker.Utility.Enum;
using Domain.Models.RewardModel;
using SubscriptionModel = Domain.Models.SubscriptionModel;
using MongoDB.Driver.Linq;
using Kusto.Cloud.Platform.Utils;

namespace Domain.Processors
{
    public class ProcessorService
    {
        private readonly Microsoft.Extensions.Logging.ILogger<ProcessorService> _logger;
        private readonly ICustomerEventService _customerEventService;
        private readonly ICampaignService _campaignService;
        private readonly ILoyaltyFraudManagementService _loyaltyFraudManagementService;
        private readonly IOfferMapService _offerMapService;
        private readonly ICumulativeTransactionService _cumulativeTransactionService;
        private readonly ICustomerSummaryService _customerSummaryService;
        private readonly WebUIDatabaseService _webUIDatabaseService;
        private readonly DBService.DBServiceClient _dBServiceClient;
        private readonly ITransactionRewardService _transactionRewardService;
        private readonly MongoService.MongoServiceClient _mongoServiceClient;
        private readonly IGroupCampaignTransactionService _groupCampaignTransactionService;
        private readonly IMissedTransactionService _missedTransactionService;

        private readonly ITransactionService _transactionService;
        private readonly IReferralService _referralService;
        private readonly ITempRewardService _tempRewardService;
        private readonly IFraudRewardService _fraudRewardService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IMerchantMaster _merchantMasterService;

        public ProcessorService
        (
            Microsoft.Extensions.Logging.ILogger<ProcessorService> logger,
            ICampaignService campaignService,
            ILoyaltyFraudManagementService loyaltyFraudManagementService,
            ICumulativeTransactionService cumulativeTransactionService,
            ICustomerEventService customerEventService,
            IOfferMapService offerMapService,
            ICustomerSummaryService customerSummaryService,
            WebUIDatabaseService webUIDatabaseService,
             DBService.DBServiceClient dBServiceClient,
            ITransactionRewardService transactionRewardService,
            MongoService.MongoServiceClient mongoServiceClient,
            ITransactionService transactionService,
            IGroupCampaignTransactionService groupCampaignTransactionService,
            IMissedTransactionService missedTransactionService,
            IReferralService referralService,
            ITempRewardService tempRewardService,
            ISubscriptionService subscriptionService,
            IFraudRewardService fraudRewardService,
            IMerchantMaster merchantMasterService
        )
        {
            _logger = logger;
            _campaignService = campaignService;
            _loyaltyFraudManagementService = loyaltyFraudManagementService;
            _cumulativeTransactionService = cumulativeTransactionService;
            _customerEventService = customerEventService;
            _offerMapService = offerMapService;
            _customerSummaryService = customerSummaryService;
            _webUIDatabaseService = webUIDatabaseService;

            _transactionRewardService = transactionRewardService;
            _dBServiceClient = dBServiceClient;
            _mongoServiceClient = mongoServiceClient;
            _transactionService = transactionService;
            _groupCampaignTransactionService = groupCampaignTransactionService;
            _missedTransactionService = missedTransactionService;
            _referralService = referralService;
            _tempRewardService = tempRewardService;
            _subscriptionService = subscriptionService;
            _fraudRewardService = fraudRewardService;
            _merchantMasterService = merchantMasterService;
        }


        public TransactionModel.ProcessedTransaction InsertIntoLoyaltyFraudConfirmedLogs(TransactionModel.ProcessedTransaction transaction)
        {
            return _loyaltyFraudManagementService.Create(transaction);
        }
        public TransactionModel.ProcessedTransaction InsertIntoCumulativeTransactionLogs(TransactionModel.ProcessedTransaction transaction)
        {
            return _cumulativeTransactionService.Create(transaction);
        }
        public List<CampaignModel.EarnCampaign> GetCampaignsUsingFilter(FilterDefinition<CampaignModel.EarnCampaign> filter)
        {
            return _campaignService.GetCampaignsUsingFilter(filter);
        }
        public List<CustomerModel.CustomerEvent> GetCustomerEvents(FilterDefinition<CustomerModel.CustomerEvent> filter)
        {
            return _customerEventService.GetCustomerEvents(filter);
        }
        public List<RewardModel.TransactionReward> GetTransactionRewards(FilterDefinition<RewardModel.TransactionReward> filter)
        {
            return _transactionRewardService.Get(filter);
        }
        public long GetCustomerEventsCount(FilterDefinition<CustomerModel.CustomerEvent> filter)
        {
            return _customerEventService.GetCustomerEventsCount(filter);
        }
        public TransactionModel.ProcessedTransaction InsertIntoOfferMap(TransactionModel.ProcessedTransaction processedTransaction)
        {
            return _offerMapService.Create(processedTransaction);
        }

        public List<TransactionModel.Transaction> GetTransactions(string mobileNumber, CampaignModel.EarnCampaign campaign)
        {
            var filterDefinition = Builders<TransactionModel.Transaction>.Filter.Where(o => (o.MobileNumber == mobileNumber) && (o.TransactionDetail.DateTime >= campaign.StartDate) && (o.TransactionDetail.DateTime <= campaign.EndDate) && (campaign.Channel.Contains(o.ChannelCode)));
            return _transactionService.Get(filterDefinition);
        }
        public List<TransactionModel.Transaction> GetTransactionsForWalletLoad(FilterDefinition<TransactionModel.Transaction> filter)
        {
            return _transactionService.Get(filter);
        }
        // BFL-2737 Prime: BBPS transaction count issue for Bajaj Prime - Prime 
        // This change done after discussion with Rajesh sir
        public List<TransactionModel.Transaction> GetTransactionsAfterPrimeActivation(string mobileNumber, CampaignModel.EarnCampaign campaign, DateTime? primeActivationDate)
        {
            var filterDefinition = Builders<TransactionModel.Transaction>.Filter.Where(o => (o.MobileNumber == mobileNumber) && (o.TransactionDetail.DateTime >= campaign.StartDate) && (o.TransactionDetail.DateTime <= campaign.EndDate) && (campaign.Channel.Contains(o.ChannelCode)) && (o.TransactionDetail.DateTime >= primeActivationDate));
            return _transactionService.Get(filterDefinition);
        }
        public List<Referral> GetReferralTransactions(string referrerMobNumber, CampaignModel.EarnCampaign campaign, string txnEventId)
        {
            var filterDefinition = Builders<Referral>.Filter.Where(o => (o.ReferrerMobNumber == referrerMobNumber) && (o.LOB == campaign.LOB) && (o.EventId == txnEventId));
            return _referralService.Get(filterDefinition);
        }
        public List<TempReward> GetTempTransactions(string mobileNumber, CampaignModel.EarnCampaign campaign)
        {
            _logger.LogInformation($"Request to GetTempTransaction : {JsonConvert.SerializeObject(campaign)}");
            var filterDefinition = Builders<TempReward>.Filter.Where(o => (o.MobileNumber == mobileNumber) && (o.LOB == campaign.LOB) && (o.CampaignId == campaign.BFLCampaignId || o.CampaignId == campaign.Id));
            return _tempRewardService.Get(filterDefinition);
        }

        public List<FraudReward> GetFraudRewardTransactions(string mobileNumber, CampaignModel.EarnCampaign campaign)
        {
            _logger.LogInformation($"Request to GetFraudRewardTransactions : {JsonConvert.SerializeObject(campaign)}");
            var filterDefinition = Builders<FraudReward>.Filter.Where(o => (o.MobileNumber == mobileNumber) && (o.LOB == campaign.LOB) && (o.CampaignId == campaign.BFLCampaignId || o.CampaignId == campaign.Id));
            var fraudReward = _fraudRewardService.Get(filterDefinition);
            _logger.LogInformation($"Response to GetFraudTransactions : {JsonConvert.SerializeObject(fraudReward)}");
            return fraudReward;
        }

        public List<TempReward> GetTempTransactions(string mobileNumber, string campaignId)
        {
            var filterDefinition = Builders<TempReward>.Filter.Where(o => (o.MobileNumber== mobileNumber) &&(o.CampaignId== campaignId));
            return _tempRewardService.Get(filterDefinition);
        }
        public List<TempReward> GetSubscriptionTempTransactions(string mobileNumber)
        {
            var filterDefinition = Builders<TempReward>.Filter.Where(o => o.MobileNumber == mobileNumber && (String.Equals(o.RewardType, "Subscription", StringComparison.OrdinalIgnoreCase) || String.Equals(o.RewardType, "PaidSubscription", StringComparison.OrdinalIgnoreCase)));
            var subscriptionTransactions = _tempRewardService.Get(filterDefinition);
            subscriptionTransactions = subscriptionTransactions.Where(o =>
            {
                if (String.Equals(o.IssueInState, "Direct", StringComparison.OrdinalIgnoreCase))
                {
                    if(o.ExpiryDate > DateTime.Now)
                    {
                        return true;
                    }
                    return false;
                }
                else
                {
                    if (String.Equals(o.IssueInState, "Lock", StringComparison.OrdinalIgnoreCase))
                    {
                        if(o.LockExpireDate > DateTime.Now)
                        {
                            return true;
                        }
                        return false;
                    }
                    else
                    {
                        if (o.ExpiryDate > DateTime.Now)
                        {
                            return true;
                        }
                        return false;
                    }
                }
            }).ToList();
            return subscriptionTransactions;
        }

        public async Task<LapsePolicy> GetLapsePolicy(string code)
        {
            ////return _webUIDatabaseService.GetLapsePolicyByCode(code);
            var response = await _dBServiceClient.LapsePolicies_GetLapsePolicyAsync(new DBService.LapsePolicyRequest() { Code = code }).ConfigureAwait(false);
            if (response == null)
            {
                return null;
            }
            var lapsePolicy = response.Data;
            if (lapsePolicy == null)
            {
                return null;
            }
            return new LapsePolicy()
            {
                Id = lapsePolicy.Id,
                Code = lapsePolicy.Code,
                DurationType = lapsePolicy.DurationType,
                DurationValue = lapsePolicy.DurationValue,
                Name = lapsePolicy.Name
            };
            //return new LapsePolicy()
            //{
            //    DurationType = "Days",
            //    DurationValue = 180
            //};
        }
        public CustomerModel.CustomerSummary GetCustomerSummary(string mobileNumber)
        {
            return _customerSummaryService.GetByMobileNumber(mobileNumber);
        }
        public CustomerModel.Customer GetCustomer(string mobileNumber)
        {
            return _customerSummaryService.GetCustomerByMobileNumber(mobileNumber);
        }
        public async Task<List<DBEnumValue>> GetEnumValues(string masterCode = null)
        {
            return await _webUIDatabaseService.GetDBEnumValuesAsync(masterCode).ConfigureAwait(false);
        }
        public async Task<MongoService.TransactionReward> CreateTransactionReward(MongoService.TransactionReward transactionReward)
        {
            return await _mongoServiceClient.TransactionRewards_PostAsync(transactionReward).ConfigureAwait(false);
        }

        public async Task<MongoService.CustomerEvent> CreateCustomerEventAsync(MongoService.CustomerEvent customerEvent)
        {
            return await _mongoServiceClient.CustomerEvents_PostAsync(customerEvent).ConfigureAwait(false);
        }
        public async Task<CustomerModel.CustomerEvent> CreateCustomerEventAsync(CustomerModel.CustomerEvent customerEvent)
        {
            return await Task.FromResult(_customerEventService.Create(customerEvent)).ConfigureAwait(false);
        }
        public async Task<List<MongoService.CustomerMobileCampaignMapping>> GetCustomerMobileCampaignMappings(MongoService.CustomerMobileCampaignMappingRequest customerMobileCampaignMappingRequest)
        {
            return (await _mongoServiceClient.CustomerMobileCampaignMappings_GetCustomerMobileCampaignMappingsAsync(customerMobileCampaignMappingRequest).ConfigureAwait(false)).ToList();
        }

        public async Task<MongoService.CustomerMobileCampaignMapping> CreateCustomerMobileCampaignMapping(MongoService.CustomerMobileCampaignMappingRequest customerMobileCampaignMappingRequest)
        {
            return await _mongoServiceClient.CustomerMobileCampaignMappings_InsertCustomerMobileCampaignMappingsAsync(customerMobileCampaignMappingRequest).ConfigureAwait(false);
        }

        public async Task<IEnumerable<MongoService.TripleRewardCustomerMerchantMapping>> GetTripleRewardMerchantMappings(MongoService.TripleRewardCustomerMerchantMapping tripleRewardMerchantMapping)
        {
            return await _mongoServiceClient.TripleRewardCustomerMerchantMappings_FetchTripleRewardCustomerMerchantMappingAsync(tripleRewardMerchantMapping).ConfigureAwait(false);

        }
        public async Task<GroupedCampaignTransaction> GroupedCampaignTransaction(GroupedCampaignTransaction groupedCampaignTransaction)
        {
            var createGroupedCampaignTransactionResponse = _groupCampaignTransactionService.Create(groupedCampaignTransaction);
            return await Task.FromResult(createGroupedCampaignTransactionResponse).ConfigureAwait(false);
        }

        public void UpdateMissedTransaction(TransactionModel.Transaction transaction, string transactionMobileNumber)
        {
            var filterDefinition = Builders<MissedTransaction>.Filter.Where(o => o.TransactionReferenceNumber == transaction.TransactionDetail.RefNumber && o.MobileNumber == transactionMobileNumber);

            var updateDefinition = Builders<MissedTransaction>.Update
                .Set(o => o.IsQueued, true)
                .Set(o=> o.IsReceivedOnQualificationService, true)
                .Set(k=>k.QueuedDateTime,System.DateTime.Now)
                .Set(k=>k.ReceivedOnQualificationServiceDateTime,System.DateTime.Now)
                .Set(k=>k.ProcessedTransaction.TransactionRequest.TransactionDetail.Customer.CustomerType, transaction.TransactionDetail.Customer.CustomerType)
                .Set(k=>k.ProcessedTransaction.Customer.Type, transaction.TransactionDetail.Customer.CustomerType);
            
            _missedTransactionService.Update(filterDefinition, updateDefinition);
        }

        #region new 

        public bool WalletLoadCheck(ProcessedTransaction transaction, EarnCampaign campaign, WalletLoad walletLoad, string eventCode)
        {
            string preFix = $"TransactionReferenceNumber : {transaction.TransactionRequest.TransactionDetail.RefNumber} ::: ";
            _logger.LogInformation("{preFix} WalletLoadCheck Started For transactionId : {transactionId}, transactionReferenceNumber : {transactionReferenceNumber}, campaignId : {campaignId}", preFix, transaction.TransactionRequest.TransactionId, transaction.TransactionRequest.TransactionDetail.RefNumber, campaign.Id);
            var transactionRequestAmount = transaction.TransactionRequest.TransactionDetail.Amount;
            _logger.LogInformation("{preFix} transactionRequestAmount : {transactionRequestAmount}", preFix, transactionRequestAmount);
            var transactionPaymentInstrument = transaction.TransactionRequest.TransactionDetail.Payments?.Select(o => o.PaymentInstrument).ToList();
            _logger.LogInformation("transactionPaymentInstrument : {transactionPaymentInstrument}", JsonConvert.SerializeObject(transactionPaymentInstrument));
            var campaignPaymentInstrument = walletLoad.PaymentInstruments;
            _logger.LogInformation("campaignPaymentInstrument : {campaignPaymentInstrument}", JsonConvert.SerializeObject(campaignPaymentInstrument));
            var currentTransactionDateTime = transaction.TransactionRequest.TransactionDetail.DateTime;
            _logger.LogInformation("currentTransactionDateTime : {currentTransactionDateTime}", currentTransactionDateTime);

            _logger.LogInformation("walletLoad.IsMinimumLoadAmount : {isMinimumLoadAmount}", walletLoad.IsMinimumLoadAmount);

            if (walletLoad.IsMinimumLoadAmount)
            {
                var walletLoadMinimumAmountCheck = WalletLoadMinimumAmountCheck(walletLoad, transactionRequestAmount);
                _logger.LogInformation("walletLoad.IsMinimumLoadAmount => walletLoadMinimumAmountCheck : {walletLoadMinimumAmountCheck}", !walletLoadMinimumAmountCheck);
                if (!walletLoadMinimumAmountCheck)
                {
                    return false;
                }
            }

            var flag = campaignPaymentInstrument != null && !campaignPaymentInstrument.Intersect(transactionPaymentInstrument).Any();
            _logger.LogInformation("campaignPaymentInstrument != null && !campaignPaymentInstrument.Intersect(transactionPaymentInstrument).Any() : {flag}", flag);
            if (flag)
            {
                return false;
            }


            if (walletLoad.IsLoadInterval)
            {
                //var filterBuilder = Builders<RewardModel.TransactionReward>.Filter;
                var filterBuilder = Builders<CustomerEvent>.Filter;
                var walletCreationFilter = filterBuilder.PrepareWalletCreationFilter(transaction.Customer.MobileNumber);
                var walletCreationDateTime = GetWalletCreationDateTime(walletCreationFilter);
                if (walletCreationDateTime == null)
                {
                    _logger.LogInformation("$ Wallet Creation Detail Not Found");
                    return false;
                }
                TimeSpan timeSpan = currentTransactionDateTime.Subtract(Convert.ToDateTime(walletCreationDateTime));
                var calculatedInterval = Math.Ceiling(timeSpan.TotalMinutes);
                //var miliSeconds = timeSpan.TotalMilliseconds;
                //var calculatedInterval = Math.Abs(currentTransactionDateTime.Subtract(Convert.ToDateTime(walletCreationDateTime)).TotalMilliseconds);
                _logger.LogInformation($"CalculatedInterval : {calculatedInterval}");
                var configuredInterval = walletLoad.LoadIntervalHours * 60 + walletLoad.LoadIntervalMinutes;
                //var configuredInterval = Math.Abs((walletLoad.LoadIntervalHours * 60 * 60 * 1000) + (walletLoad.LoadIntervalMinutes * 60 * 1000));
                _logger.LogInformation($"ConfiguredInterval : {configuredInterval}");

                _logger.LogInformation($"calculatedInterval is Less than Or Equal configuredInterval : {calculatedInterval <= configuredInterval}");
                if (calculatedInterval > configuredInterval)
                {
                    _logger.LogInformation($"calculatedInterval Is Greater that Configured Interval : {calculatedInterval > configuredInterval}");
                    return false;
                }
            }
            _logger.LogInformation("WalletLoad.LoadCount  : {0}", walletLoad.LoadCount);
            if (walletLoad.LoadCount == 0)
            {
                walletLoad.LoadCount = 1;
            }

            long walletLoadCount = GetWalletLoadCountByFilter(preFix, transaction, campaign, walletLoad);
            _logger.LogInformation("{preFix} WalletLoadCount : {walletLoadCount}", preFix, walletLoadCount);
            _logger.LogInformation("{preFix} walletLoadCount > 0 && (walletLoadCount + 1) == (long)walletLoad.LoadCount : {0}", preFix, (walletLoadCount > 0 && (walletLoadCount + 1) == (long)walletLoad.LoadCount));
            if ((walletLoadCount + 1) == (long)walletLoad.LoadCount)
            {
                _logger.LogInformation("WalletLoadCheck End For transactionId : {transactionId}, transactionReferenceNumber : {transactionReferenceNumber}, campaignId : {campaignId}", transaction.TransactionRequest.TransactionId, transaction.TransactionRequest.TransactionDetail.RefNumber, campaign.Id);
                return true;
            }
            _logger.LogInformation("WalletLoadCheck End For transactionId : {transactionId}, transactionReferenceNumber : {transactionReferenceNumber}, campaignId : {campaignId}", transaction.TransactionRequest.TransactionId, transaction.TransactionRequest.TransactionDetail.RefNumber, campaign.Id);
            return false;

            //if (walletLoad.LoadCount == 1)
            //{
            //    //_logger.LogInformation("WalletLoadCheck Started For {transactionId}, {transactionReferenceNumber}, {campaignId}", transaction.TransactionRequest.TransactionId, transaction.TransactionRequest.TransactionDetail.RefNumber, campaign.Id);
            //    //return true;

            //    long walletLoadCount = GetWalletLoadCountByFilter(transaction, campaign, walletLoad);

            //    _logger.LogInformation("WalletLoadCount : {walletLoadCount}", walletLoadCount);
            //    _logger.LogInformation("(walletLoadCount + 1) == (long)walletLoad.LoadCount : {0}", (walletLoadCount > 0 && (walletLoadCount + 1) == (long)walletLoad.LoadCount));
            //    if ((walletLoadCount + 1) == (long)walletLoad.LoadCount)
            //    {
            //        _logger.LogInformation("WalletLoadCheck Started For {transactionId}, {transactionReferenceNumber}, {campaignId}", transaction.TransactionRequest.TransactionId, transaction.TransactionRequest.TransactionDetail.RefNumber, campaign.Id);
            //        return true;
            //    }
            //    _logger.LogInformation("WalletLoadCheck Started For {transactionId}, {transactionReferenceNumber}, {campaignId}", transaction.TransactionRequest.TransactionId, transaction.TransactionRequest.TransactionDetail.RefNumber, campaign.Id);
            //    return false;
            //}
            //else
            //{
            //    long walletLoadCount = GetWalletLoadCountByFilter(transaction, campaign, walletLoad);

            //    _logger.LogInformation("WalletLoadCount : {walletLoadCount}", walletLoadCount);
            //    _logger.LogInformation("walletLoadCount > 0 && (walletLoadCount + 1) == (long)walletLoad.LoadCount : {0}", (walletLoadCount > 0 && (walletLoadCount + 1) == (long)walletLoad.LoadCount));
            //    if (walletLoadCount > 0 && (walletLoadCount + 1) == (long)walletLoad.LoadCount)
            //    {
            //        _logger.LogInformation("WalletLoadCheck Started For {transactionId}, {transactionReferenceNumber}, {campaignId}", transaction.TransactionRequest.TransactionId, transaction.TransactionRequest.TransactionDetail.RefNumber, campaign.Id);
            //        return true;
            //    }
            //    _logger.LogInformation("WalletLoadCheck Started For {transactionId}, {transactionReferenceNumber}, {campaignId}", transaction.TransactionRequest.TransactionId, transaction.TransactionRequest.TransactionDetail.RefNumber, campaign.Id);
            //    return false;
            //}

            DateTime? GetWalletCreationDateTime(FilterDefinition<Domain.Models.CustomerModel.CustomerEvent> filter)
            {
                DateTime? walletCreationDateTime = null;
                //var transactionRewards = _processorService.GetTransactionRewards(filter);
                var customerEvents = _customerEventService.GetCustomerEvents(filter);
                if (customerEvents != null && customerEvents.Any())
                {
                    var customerEvent = customerEvents.OrderByDescending(o => o.TxnDateTime).FirstOrDefault();
                    if (customerEvent != null)
                    {
                        walletCreationDateTime = customerEvent.TxnDateTime;
                    }
                }
                return walletCreationDateTime;
            }
            long GetWalletLoadCount(string prefix, FilterDefinition<Domain.Models.CustomerModel.CustomerEvent> filter)
            {
                var walletLoadCount = 0;
                var customerEvents = GetCustomerEvents(filter).Where(o => o.PaymentInstrument.Intersect(walletLoad.PaymentInstruments).Any()).ToList();

                _logger.LogInformation("customerEvents : {customerEvents}", JsonConvert.SerializeObject(customerEvents));

                if (customerEvents != null && customerEvents.Any())
                {
                    walletLoadCount = customerEvents.Count;
                }

                _logger.LogInformation("GetWalletLoadCount =>  walletLoadCount => : {walletLoadCount}", walletLoadCount);

                return walletLoadCount;
                //return _processorService.GetCustomerEventsCount(filter);
            }
            //static bool WalletLoadMinimumAmountCheck(CampaignModel.WalletLoad walletLoad, double transactionRequestAmount)
            //{
            //    return !walletLoad.IsMinimumLoadAmount || transactionRequestAmount >= Convert.ToDouble(walletLoad.MinimumLoadAmount);
            //}
            static bool WalletLoadMinimumAmountCheck(Domain.Models.CampaignModel.WalletLoad walletLoad, double transactionRequestAmount)
            {
                return walletLoad.IsMinimumLoadAmount && (transactionRequestAmount >= Convert.ToDouble(walletLoad.MinimumLoadAmount));
            }

            long GetWalletLoadCountByFilter(string preFix, Domain.Models.TransactionModel.ProcessedTransaction transaction, Domain.Models.CampaignModel.EarnCampaign campaign, WalletLoad walletLoad)
            {
                var filterBuilder = Builders<Domain.Models.CustomerModel.CustomerEvent>.Filter;
                var walletLoadFilter = filterBuilder.PrepareWalletLoadFilter(transaction.Customer.MobileNumber, campaign.StartDate, walletLoad);
                var walletLoadCount = GetWalletLoadCount(preFix, walletLoadFilter);
                return walletLoadCount;
            }
        }

        public bool UPILiteCheck(ProcessedTransaction transaction, EarnCampaign campaign, UPILite upiLite, string eventCode)
        {
            string preFix = $"TransactionReferenceNumber : {transaction.TransactionRequest.TransactionDetail.RefNumber} ::: ";
            var isQualified = true;
            _logger.LogInformation("UPILiteCheck Started For transactionId : {transactionId}, transactionReferenceNumber : {transactionReferenceNumber}, campaignId : {campaignId}", transaction.TransactionRequest.TransactionId, transaction.TransactionRequest.TransactionDetail.RefNumber, campaign.Id);
            var transactionRequestAmount = transaction.TransactionRequest.TransactionDetail.Amount;
            _logger.LogInformation("transactionRequestAmount : {transactionRequestAmount}", transactionRequestAmount);
            var transactionPaymentInstrument = transaction.TransactionRequest.TransactionDetail.Payments?.Select(o => o.PaymentInstrument).ToList();
            _logger.LogInformation("transactionPaymentInstrument : {transactionPaymentInstrument}", JsonConvert.SerializeObject(transactionPaymentInstrument));
            var campaignPaymentInstrument = upiLite.PaymentInstruments;
            _logger.LogInformation("campaignPaymentInstrument : {campaignPaymentInstrument}", JsonConvert.SerializeObject(campaignPaymentInstrument));
            var currentTransactionDateTime = transaction.TransactionRequest.TransactionDetail.DateTime;
            _logger.LogInformation("currentTransactionDateTime : {currentTransactionDateTime}", currentTransactionDateTime);

            _logger.LogInformation("upiLite.IsMinimumAmount : {IsMinimumAmount}", upiLite.IsMinimumAmount);
            _logger.LogInformation("upiLite.IsLoadCount : {IsLoadCount}", upiLite.IsLoadCount);

            var isTransactionUPI = transactionPaymentInstrument != null && transactionPaymentInstrument.Contains("UPI");
            _logger.LogInformation("transactionPaymentInstrument != null && transactionPaymentInstrument.Contains(UPI) : {isTransactionUPI}", isTransactionUPI);

            var flag = !isTransactionUPI;
            _logger.LogInformation($"flag ==> {flag}");

            if (flag)
            {
                _logger.LogInformation("Transaction payment instrument is not UPI. Returning false.");
                return false;
            }

            _logger.LogInformation($"Checking first condition: !upiLite.IsMinimumAmount = {!upiLite.IsMinimumAmount}, !upiLite.IsLoadCount = {!upiLite.IsLoadCount}, flag = {!flag}");

            if (!upiLite.IsMinimumAmount && !upiLite.IsLoadCount && !flag)
            {
                _logger.LogInformation("Both IsMinimumAmount and IsLoadCount are false. Returning true.");
                return true;
            }

            _logger.LogInformation($"Checking second condition: upiLite.IsMinimumAmount = {upiLite.IsMinimumAmount}, !upiLite.IsLoadCount = {!upiLite.IsLoadCount}, flag = {!flag}");
            if (upiLite.IsMinimumAmount && !upiLite.IsLoadCount && !flag)
            {
                var upiLiteMinimumAmountCheck = UPILiteMinimumAmountCheck(upiLite, transactionRequestAmount);
                _logger.LogInformation("upiLite.IsMinimumAmount => upiLiteMinimumAmountCheck : {upiLiteMinimumAmountCheck}", !upiLiteMinimumAmountCheck);
                if (!upiLiteMinimumAmountCheck)
                {
                    _logger.LogInformation("UPILiteMinimumAmountCheck failed. Returning false.");
                    return false;
                }
                _logger.LogInformation("UPILiteMinimumAmountCheck passed. Returning true.");
                return true;
            }

            _logger.LogInformation($"Checking third condition: !upiLite.IsMinimumAmount = {!upiLite.IsMinimumAmount}, upiLite.IsLoadCount = {upiLite.IsLoadCount}, flag = {!flag}");
            if (!upiLite.IsMinimumAmount && upiLite.IsLoadCount && !flag)
            {
                var configuredUPILiteEvent = eventCode;
                var customerTransactions = new List<Domain.Models.TransactionModel.Transaction>();
                if (upiLite.IsLoadCount && upiLite.LoadCount > 0)
                {
                    if ((campaign.SubscriptionTypes.Contains("Paid") || campaign.SubscriptionTypes.Contains("Trial")) && campaign.SubscriptionTypes.Contains("regular"))
                    {
                        customerTransactions = GetTransactions(transaction.Customer.MobileNumber, campaign);
                        _logger.LogInformation($"Total customerTransactions count after calling GetTransactions() with basic filter: {customerTransactions.Count} with  campaign.BFLCampaignId :{campaign.BFLCampaignId}");

                    }
                    else if (campaign.SubscriptionTypes.Contains("Paid") && campaign.SubscriptionTypes.Contains("Trial") && !campaign.SubscriptionTypes.Contains("regular"))
                    {
                        customerTransactions = GetTransactions(transaction.Customer.MobileNumber, campaign);
                        _logger.LogInformation($"Total customerTransactions count after calling GetTransactions() with basic filter: {customerTransactions.Count} with  campaign.BFLCampaignId :{campaign.BFLCampaignId}");
                    }
                    else if (campaign.SubscriptionTypes.Contains("Paid") || campaign.SubscriptionTypes.Contains("Trial"))
                    {
                        customerTransactions = GetTransactionsAfterPrimeActivation(transaction.Customer.MobileNumber, campaign, transaction.Customer.PrimeActivationDate);
                        _logger.LogInformation($"Total customerTransactions count after calling GetTransactionsAfterPrimeActivation() with basic filter: {customerTransactions.Count} with  campaign.BFLCampaignId :{campaign.BFLCampaignId}");
                    }
                    else
                    {
                        customerTransactions = GetTransactions(transaction.Customer.MobileNumber, campaign);
                        _logger.LogInformation($"Total customerTransactions count after calling GetTransactions() with basic filter: {customerTransactions.Count} with  campaign.BFLCampaignId :{campaign.BFLCampaignId}");
                    }

                    var filteredCustomerTransactions = customerTransactions
                                                .Where(o => configuredUPILiteEvent.Contains(o.EventId))
                                                .Where(o => (o.TransactionDetail.Payments.Any(p => p.PaymentInstrument == campaignPaymentInstrument)))
                                                .Where(o => !upiLite.IsMinimumAmount || o.TransactionDetail.Amount >= (double)upiLite.MinimumAmount)
                                                .Where(o => o.TransactionDetail.DateTime <= currentTransactionDateTime)
                                                .ToList();

                    _logger.LogInformation($"Total filteredCustomerTransactions count after scategory,paymentinstruments and amount  filter: {filteredCustomerTransactions.Count} with  campaign.BFLCampaignId :{campaign.BFLCampaignId}");

                    var totalTransactionCount = 0;

                    _logger.LogInformation($"Before UPILite : totalTransactionCount :: {totalTransactionCount}");
                    totalTransactionCount = filteredCustomerTransactions.Count;
                    _logger.LogInformation($"After UPILite : totalTransactionCount :: {totalTransactionCount}");

                    _logger.LogInformation($"TotalTransactionCount: {totalTransactionCount} == upiLite.LoadCount: {upiLite.LoadCount}");

                    if (upiLite.IsLoadCount)
                    {
                        _logger.LogInformation($"Value of IsLoadCount : {upiLite.IsLoadCount}, CountType: {upiLite.CountType}, TotalTransactionCount : {totalTransactionCount}, upiLite.LoadCount: {upiLite.LoadCount}");
                        if (upiLite.CountType == 3 && totalTransactionCount != upiLite.LoadCount)
                        {
                            _logger.LogInformation($"skipping campaign not qualified: CountType is {upiLite.CountType} (equal)");
                            isQualified = false;
                            return isQualified;
                        }

                        if (upiLite.CountType == 2 && totalTransactionCount <= upiLite.LoadCount)
                        {
                            _logger.LogInformation($"skipping campaign not qualified: CountType is {upiLite.CountType} (greater than)");
                            isQualified = false;
                            return isQualified;
                        }

                        if (upiLite.CountType == 1 && totalTransactionCount >= upiLite.LoadCount)
                        {
                            _logger.LogInformation($"skipping campaign not qualified: CountType is {upiLite.CountType} (less than)");
                            isQualified = false;
                            return isQualified;
                        }
                    }
                }
            }

            _logger.LogInformation($"Checking fourth condition: upiLite.IsMinimumAmount = {upiLite.IsMinimumAmount}, upiLite.IsLoadCount = {upiLite.IsLoadCount}, flag = {!flag}");
            if (upiLite.IsMinimumAmount && upiLite.IsLoadCount && !flag)
            {
                var upiLiteMinimumAmountCheck = UPILiteMinimumAmountCheck(upiLite, transactionRequestAmount);
                _logger.LogInformation("upiLite.IsMinimumAmount => upiLiteMinimumAmountCheck : {upiLiteMinimumAmountCheck}", !upiLiteMinimumAmountCheck);
                if (!upiLiteMinimumAmountCheck)
                {
                    _logger.LogInformation("UPILiteMinimumAmountCheck failed. Returning false.");
                    return false;
                }
                _logger.LogInformation("UPILiteMinimumAmountCheck passed. Returning true.");

                var configuredUPILiteEvent = eventCode;
                var customerTransactions = new List<Domain.Models.TransactionModel.Transaction>();
                if (upiLite.IsLoadCount && upiLite.LoadCount > 0)
                {
                    if ((campaign.SubscriptionTypes.Contains("Paid") || campaign.SubscriptionTypes.Contains("Trial")) && campaign.SubscriptionTypes.Contains("regular"))
                    {
                        customerTransactions = GetTransactions(transaction.Customer.MobileNumber, campaign);
                        _logger.LogInformation($"Total customerTransactions count after calling GetTransactions() with basic filter: {customerTransactions.Count} with  campaign.BFLCampaignId :{campaign.BFLCampaignId}");

                    }
                    else if (campaign.SubscriptionTypes.Contains("Paid") && campaign.SubscriptionTypes.Contains("Trial") && !campaign.SubscriptionTypes.Contains("regular"))
                    {
                        customerTransactions = GetTransactions(transaction.Customer.MobileNumber, campaign);
                        _logger.LogInformation($"Total customerTransactions count after calling GetTransactions() with basic filter: {customerTransactions.Count} with  campaign.BFLCampaignId :{campaign.BFLCampaignId}");
                    }
                    else if (campaign.SubscriptionTypes.Contains("Paid") || campaign.SubscriptionTypes.Contains("Trial"))
                    {
                        customerTransactions = GetTransactionsAfterPrimeActivation(transaction.Customer.MobileNumber, campaign, transaction.Customer.PrimeActivationDate);
                        _logger.LogInformation($"Total customerTransactions count after calling GetTransactionsAfterPrimeActivation() with basic filter: {customerTransactions.Count} with  campaign.BFLCampaignId :{campaign.BFLCampaignId}");
                    }
                    else
                    {
                        customerTransactions = GetTransactions(transaction.Customer.MobileNumber, campaign);
                        _logger.LogInformation($"Total customerTransactions count after calling GetTransactions() with basic filter: {customerTransactions.Count} with  campaign.BFLCampaignId :{campaign.BFLCampaignId}");
                    }

                    var filteredCustomerTransactions = customerTransactions
                                                .Where(o => configuredUPILiteEvent.Contains(o.EventId))
                                                .Where(o => (o.TransactionDetail.Payments.Any(p => p.PaymentInstrument == campaignPaymentInstrument)))
                                                .Where(o => !upiLite.IsMinimumAmount || o.TransactionDetail.Amount >= (double)upiLite.MinimumAmount)
                                                .Where(o => o.TransactionDetail.DateTime <= currentTransactionDateTime)
                                                .ToList();

                    _logger.LogInformation($"Total filteredCustomerTransactions count after scategory,paymentinstruments and amount  filter: {filteredCustomerTransactions.Count} with  campaign.BFLCampaignId :{campaign.BFLCampaignId}");

                    var totalTransactionCount = 0;

                    _logger.LogInformation($"Before UPILite : totalTransactionCount :: {totalTransactionCount}");
                    totalTransactionCount = filteredCustomerTransactions.Count;
                    _logger.LogInformation($"After UPILite : totalTransactionCount :: {totalTransactionCount}");

                    _logger.LogInformation($"TotalTransactionCount: {totalTransactionCount} == upiLite.LoadCount: {upiLite.LoadCount}");

                    if (upiLite.IsLoadCount)
                    {
                        _logger.LogInformation($"Value of IsLoadCount : {upiLite.IsLoadCount}, CountType: {upiLite.CountType}, TotalTransactionCount : {totalTransactionCount}, upiLite.LoadCount: {upiLite.LoadCount}");
                        if (upiLite.CountType == 3 && totalTransactionCount != upiLite.LoadCount)
                        {
                            _logger.LogInformation($"skipping campaign not qualified: CountType is {upiLite.CountType} (equal)");
                            isQualified = false;
                            return isQualified;
                        }

                        if (upiLite.CountType == 2 && totalTransactionCount <= upiLite.LoadCount)
                        {
                            _logger.LogInformation($"skipping campaign not qualified: CountType is {upiLite.CountType} (greater than)");
                            isQualified = false;
                            return isQualified;
                        }

                        if (upiLite.CountType == 1 && totalTransactionCount >= upiLite.LoadCount)
                        {
                            _logger.LogInformation($"skipping campaign not qualified: CountType is {upiLite.CountType} (less than)");
                            isQualified = false;
                            return isQualified;
                        }
                    }
                }
            }

            _logger.LogInformation("End Checking IsMinimumAmount & IsLoadCount");

            _logger.LogInformation("UPILiteCheck End For transactionId : {transactionId}, transactionReferenceNumber : {transactionReferenceNumber}, campaignId : {campaignId}", transaction.TransactionRequest.TransactionId, transaction.TransactionRequest.TransactionDetail.RefNumber, campaign.Id);
            return isQualified;

            static bool UPILiteMinimumAmountCheck(Domain.Models.CampaignModel.UPILite upiLite, double transactionRequestAmount)
            {
                return upiLite.IsMinimumAmount && (transactionRequestAmount >= (double)upiLite.MinimumAmount);
            }
        }

        public List<RewardOptionType> GetRewardOptions(EarnCampaign campaign)
        {
            List<RewardOptionType> rewardOptionTypes = new List<RewardOptionType>();
            foreach (var rewardOption in campaign.RewardOption)
            {
                if (string.Equals(rewardOption.RewardType, "Points", StringComparison.OrdinalIgnoreCase))
                {
                    string durationType = "Days";
                    int durationValue = 0;

                    if (!String.IsNullOrEmpty(rewardOption.Points.ExpirePolicy))
                    {
                        var lapsePolicy = GetLapsePolicy(rewardOption.Points.ExpirePolicy).GetAwaiter().GetResult();
                        if (lapsePolicy != null)
                        {
                            durationType = lapsePolicy.DurationType;
                            durationValue = lapsePolicy.DurationValue;
                        }
                    }
                    rewardOption.Points.DurationType = durationType;
                    rewardOption.Points.DurationValue = durationValue;
                }
                else if (string.Equals(rewardOption.RewardType, "Randomized", StringComparison.OrdinalIgnoreCase)  && (rewardOption.IsMultipleCurrency == 1))
                {
                    string durationType = "Days";
                    int durationValue = 0;

                    if(!String.IsNullOrEmpty(rewardOption.MultiCurrencyRandomizedDetails.PointCurrency.ExpirePolicy))
                    {
                        
                        var lapsePolicy = GetLapsePolicy(rewardOption.MultiCurrencyRandomizedDetails.PointCurrency.ExpirePolicy).GetAwaiter().GetResult();
                        if (lapsePolicy != null)
                        {
                            durationType = lapsePolicy.DurationType;
                            durationValue = lapsePolicy.DurationValue;
                        }
                    }
                    rewardOption.MultiCurrencyRandomizedDetails.PointCurrency.DurationType = durationType;
                    rewardOption.MultiCurrencyRandomizedDetails.PointCurrency.DurationValue = durationValue;
                }

                rewardOptionTypes.Add(rewardOption);
            }

            return rewardOptionTypes;
        }

        public List<RewardOptionType> GetMembershipRewardOptions(EarnCampaign campaign)
        {
            List<RewardOptionType> rewardOptionTypes = new List<RewardOptionType>();
            foreach (var rewardOption in campaign.MembershipReward.SubscriptionBasedRewards.SelectMany(m => m.RewardOption).ToList())
            {
                if (string.Equals(rewardOption.RewardType, "Points", StringComparison.OrdinalIgnoreCase))
                {
                    string durationType = "Days";
                    int durationValue = 0;

                    if (!String.IsNullOrEmpty(rewardOption.Points.ExpirePolicy))
                    {
                        var lapsePolicy = GetLapsePolicy(rewardOption.Points.ExpirePolicy).GetAwaiter().GetResult();
                        if (lapsePolicy != null)
                        {
                            durationType = lapsePolicy.DurationType;
                            durationValue = lapsePolicy.DurationValue;
                        }
                    }
                    rewardOption.Points.DurationType = durationType;
                    rewardOption.Points.DurationValue = durationValue;
                }
                else if (string.Equals(rewardOption.RewardType, "Randomized", StringComparison.OrdinalIgnoreCase) && (rewardOption.IsMultipleCurrency == 1))
                {
                    string durationType = "Days";
                    int durationValue = 0;

                    if (!String.IsNullOrEmpty(rewardOption.MultiCurrencyRandomizedDetails.PointCurrency.ExpirePolicy))
                    {

                        var lapsePolicy = GetLapsePolicy(rewardOption.MultiCurrencyRandomizedDetails.PointCurrency.ExpirePolicy).GetAwaiter().GetResult();
                        if (lapsePolicy != null)
                        {
                            durationType = lapsePolicy.DurationType;
                            durationValue = lapsePolicy.DurationValue;
                        }
                    }
                    rewardOption.MultiCurrencyRandomizedDetails.PointCurrency.DurationType = durationType;
                    rewardOption.MultiCurrencyRandomizedDetails.PointCurrency.DurationValue = durationValue;
                }

                rewardOptionTypes.Add(rewardOption);
            }

            return rewardOptionTypes;
        }

        public List<RewardOptionType> GetReferralRewardOptions(EarnCampaign campaign)
        {
            List<RewardOptionType> rewardOptionTypes = new List<RewardOptionType>();
            var rewardOption = campaign.RewardCriteria?.RefferalProgram?.RewardOptionType;
            {
                if (string.Equals(rewardOption.RewardType, "Points", StringComparison.OrdinalIgnoreCase))
                {
                    string durationType = "Days";
                    int durationValue = 0;

                    if (!String.IsNullOrEmpty(rewardOption.Points.ExpirePolicy))
                    {
                        var lapsePolicy = GetLapsePolicy(rewardOption.Points.ExpirePolicy).GetAwaiter().GetResult();
                        if (lapsePolicy != null)
                        {
                            durationType = lapsePolicy.DurationType;
                            durationValue = lapsePolicy.DurationValue;
                        }
                    }
                    rewardOption.Points.DurationType = durationType;
                    rewardOption.Points.DurationValue = durationValue;
                }
                else if (string.Equals(rewardOption.RewardType, "Randomized", StringComparison.OrdinalIgnoreCase) && (rewardOption.IsMultipleCurrency == 1))
                {
                    string durationType = "Days";
                    int durationValue = 0;

                    if (!String.IsNullOrEmpty(rewardOption.MultiCurrencyRandomizedDetails.PointCurrency.ExpirePolicy))
                    {

                        var lapsePolicy = GetLapsePolicy(rewardOption.MultiCurrencyRandomizedDetails.PointCurrency.ExpirePolicy).GetAwaiter().GetResult();
                        if (lapsePolicy != null)
                        {
                            durationType = lapsePolicy.DurationType;
                            durationValue = lapsePolicy.DurationValue;
                        }
                    }
                    rewardOption.MultiCurrencyRandomizedDetails.PointCurrency.DurationType = durationType;
                    rewardOption.MultiCurrencyRandomizedDetails.PointCurrency.DurationValue = durationValue;
                }
                rewardOptionTypes.Add(rewardOption);
            }

            return rewardOptionTypes;
        }

        public bool IsBillerValid(BBPS currentCampaignBiller, Domain.Models.Common.TransactionModel.Biller bbpsTypeCutomerTransactionBiller)
        {
            var flag = true;
            foreach (var billerCategory in currentCampaignBiller.BillerCategories)
            {
                var currentBillerCategory = billerCategory.BillerCategory;
                var currentBillerIds = billerCategory.Biller.Split(',').Select(id => id.Trim()).ToList();

                // Check if category matches and biller is either "Any" or matches one of the comma-separated IDs
                if (!(currentBillerCategory == bbpsTypeCutomerTransactionBiller.Category &&
                    (currentBillerIds.Contains("Any") || currentBillerIds.Contains(bbpsTypeCutomerTransactionBiller.Id))))
                {
                    flag = false;
                }
                else
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        public bool ValidateMerchantSegment(Merchant merchant, Domain.Models.Common.TransactionModel.TransactionDetail transactionDetail)
        {
            var flag = false;

            string preFix = $"TransactionReferenceNumber : {transactionDetail.RefNumber} ::: ";
            var isInclude = merchant.Segment.IsIncluded;
            _logger.LogInformation($"Campaign : isInclude : {isInclude}");
            var segments = merchant.Segment.SegmentCode;
            var merchantType = merchant.MerchantType;
            _logger.LogInformation($"Campaign : segments : {JsonConvert.SerializeObject(segments)}");
            var transactionMerchantId = transactionDetail?.MerchantOrDealer?.Id ?? string.Empty;
            _logger.LogInformation($"Transaction : transactionMerchantId : {transactionMerchantId}");
            var merchantSegmentCodes = string.Empty;
            if (segments != null)
            {
                merchantSegmentCodes = String.Join(",", segments);
            }
            _logger.LogInformation($"merchantSegmentCodes : merchantSegmentCodes : {merchantSegmentCodes}");
            if (merchant.MerchantSegment == "Any")
            {
                merchantSegmentCodes = string.Empty;
            }
            _logger.LogInformation($"{preFix} MerchantSegmentCodes: Query : {merchantSegmentCodes}");
            var isMerchantExist = _webUIDatabaseService.IsMerchantExist(preFix, merchantSegmentCodes, transactionMerchantId, merchantType);
            _logger.LogInformation($"{preFix} isMerchantExist:  : {isMerchantExist}");
            bool isExistInMerchantType = false;
            if (!isInclude)
            {
                _logger.LogInformation($"{preFix} Exclusion :: isInclude:  : {isInclude}");
                isExistInMerchantType = _webUIDatabaseService.IsMerchantExist(preFix, string.Empty, transactionMerchantId, merchantType); ;
                _logger.LogInformation($"{preFix} isExistInMerchantType:  : {isExistInMerchantType}");
            }

            //var merchantDetail = _merchantMasterService.GetMerchantMasterValues(new MerchantEnumRequest
            //{
            //    MerchantId = transactionDetail.MerchantOrDealer.Id,
            //    Category = transactionDetail.MerchantOrDealer.Category,
            //    GroupMerchantId = transactionDetail.MerchantOrDealer.GroupId,
            //    Source = transactionDetail.MerchantOrDealer.Source
            //});
            //merchant exist check 
            //P2M
            //P2p -> P2pL, P2pU
            //bool merchantTypeMatched = string.Equals(merchantDetail.MType, merchant.MerchantType, StringComparison.OrdinalIgnoreCase);
            //_logger.LogInformation($"{preFix} Merchant Type : {merchantDetail.MType}, Campaign Merchant Type : {merchant.MerchantType} -- Matched : {merchantTypeMatched}");

            if (merchant.MerchantSegment == "Any" && isMerchantExist)
            {
                flag = true;
            }
            else if (merchant.MerchantSegment != "Any")
            {
                if (isInclude)
                {
                    if (isMerchantExist)
                    {
                        flag = true;
                    }
                }
                else
                {
                    if (!isMerchantExist && isExistInMerchantType)
                    {
                        _logger.LogInformation($"{preFix} Exclusion:  : {isMerchantExist}");
                        flag = true;
                    }
                }
            }
            else
            {
                flag = false;
            }
            _logger.LogInformation($"Final Result : flag: {flag}");
            return flag;
        }


        #endregion


        #region vpa
        public bool ApplyVPASegmentFilter(EarnCampaign campaign, Domain.Models.Common.TransactionModel.TransactionDetail TransactionDetails, string type, string offerType, string prefix, string rewardIssuance = "")
        {
            _logger.LogInformation("============================start ApplyVPASegmentFilter===============================");

            bool isQualified = false;
            try
            {

                string DestinationId = TransactionDetails?.Customer?.DestinationVPAId;
                string _type = TransactionDetails?.Type;
                string campaignVpaSegmentType = "";
                List<string> campaignVpaSegmentList = new List<string>();

                // InCase of Vpa Offline
                if (offerType == "Payment" && type == "P2MOF")
                {
                    if (rewardIssuance == "Direct")
                    {
                        campaignVpaSegmentType = campaign?.RewardCriteria?.Direct?.Event?.GetPaymentDirect()?.VpaMerchantSegment?.VpaSegmentOffline;
                        campaignVpaSegmentList = campaign?.RewardCriteria?.Direct?.Event?.GetPaymentDirect()?.VpaMerchantSegment?.LstVpaSegmentOffline;
                    }
                    else if (rewardIssuance == "Lock")
                    {
                        campaignVpaSegmentType = campaign?.RewardCriteria?.WithLockUnlock?.LockEvent?.Event?.GetPaymentDirect()?.VpaMerchantSegment?.VpaSegmentOffline;
                        campaignVpaSegmentList = campaign?.RewardCriteria?.WithLockUnlock?.LockEvent?.Event?.GetPaymentDirect()?.VpaMerchantSegment?.LstVpaSegmentOffline;
                    }
                    else if (rewardIssuance == "Unlock")
                    {
                        campaignVpaSegmentType = campaign?.RewardCriteria?.WithLockUnlock?.UnlockEvent?.Event?.GetPaymentDirect()?.VpaMerchantSegment?.VpaSegmentOffline;
                        campaignVpaSegmentList = campaign?.RewardCriteria?.WithLockUnlock?.UnlockEvent?.Event?.GetPaymentDirect()?.VpaMerchantSegment?.LstVpaSegmentOffline;
                    }
                }

                // InCase of Vpa Online
                if (offerType == "Payment" && type == "P2MON")
                {
                    if (rewardIssuance == "Direct")
                    {
                        campaignVpaSegmentType = campaign?.RewardCriteria?.Direct?.Event?.GetPaymentDirect()?.VpaMerchantSegment?.VpaSegmentOnline;
                        campaignVpaSegmentList = campaign?.RewardCriteria?.Direct?.Event?.GetPaymentDirect()?.VpaMerchantSegment?.LstVpaSegmentOnline;
                    }
                    else if (rewardIssuance == "Lock")
                    {
                        campaignVpaSegmentType = campaign?.RewardCriteria?.WithLockUnlock?.LockEvent?.Event?.GetPaymentDirect()?.VpaMerchantSegment?.VpaSegmentOnline;
                        campaignVpaSegmentList = campaign?.RewardCriteria?.WithLockUnlock?.LockEvent?.Event?.GetPaymentDirect()?.VpaMerchantSegment?.LstVpaSegmentOnline;
                    }
                    else if (rewardIssuance == "Unlock")
                    {
                        campaignVpaSegmentType = campaign?.RewardCriteria?.WithLockUnlock?.UnlockEvent?.Event?.GetPaymentDirect()?.VpaMerchantSegment?.VpaSegmentOnline;
                        campaignVpaSegmentList = campaign?.RewardCriteria?.WithLockUnlock?.UnlockEvent?.Event?.GetPaymentDirect()?.VpaMerchantSegment?.LstVpaSegmentOnline;
                    }
                }

                if (offerType == "Activity" && type == "P2MON")
                {
                    if (rewardIssuance == "Direct")
                    {
                        campaignVpaSegmentType = campaign?.RewardCriteria?.Direct?.Event?.GetEMandateCreation()?.VpaSegment?.VpaSegmentOnline;
                        campaignVpaSegmentList = campaign?.RewardCriteria?.Direct?.Event?.GetEMandateCreation()?.VpaSegment?.LstVpaSegmentOnline;
                    }
                    else if (rewardIssuance == "Lock")
                    {
                        campaignVpaSegmentType = campaign?.RewardCriteria?.WithLockUnlock?.LockEvent?.Event?.GetEMandateCreation()?.VpaSegment?.VpaSegmentOnline;
                        campaignVpaSegmentList = campaign?.RewardCriteria?.WithLockUnlock?.LockEvent?.Event?.GetEMandateCreation()?.VpaSegment?.LstVpaSegmentOnline;
                    }
                    else if (rewardIssuance == "Unlock")
                    {
                        campaignVpaSegmentType = campaign?.RewardCriteria?.WithLockUnlock?.UnlockEvent?.Event?.GetEMandateCreation()?.VpaSegment?.VpaSegmentOnline;
                        campaignVpaSegmentList = campaign?.RewardCriteria?.WithLockUnlock?.UnlockEvent?.Event?.GetEMandateCreation()?.VpaSegment?.LstVpaSegmentOnline;
                    }

                    _logger.LogInformation($"DestinationId : {DestinationId} --> campaignVpaSegmentType:{campaignVpaSegmentType} --> campaignVpaSegmentList:{campaignVpaSegmentList}");

                    if (String.IsNullOrEmpty(campaignVpaSegmentType))
                    {
                        _logger.LogInformation($"{prefix} Campaign VPASegment is null or empty : {campaignVpaSegmentType}--> campaign:{campaign}");
                        isQualified = true;
                        return isQualified;

                    }
                    else if (campaignVpaSegmentType == CampaignEnum.No.ToString())
                    {
                        isQualified = true;
                        return isQualified;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(_type))
                        {
                            _type = type.ToLower();
                        }

                        var inQuery = "'" + string.Join("', '", campaignVpaSegmentList.Select(s => s)) + "'";
                        var query = @" WHERE s.`Type`= '" + _type + "'AND  cs.`DestinationVpaID`='" + DestinationId + "' AND s.`VPAMerchantSegmentCode` IN (" + inQuery + ")";


                        _logger.LogInformation($"{prefix} Query : {query}");

                        var matchedSegmentCount = _webUIDatabaseService.GetVPACustomerSegmentCount(query);
                        _logger.LogInformation($"{prefix} MatchedSegmentCount : {matchedSegmentCount}");

                        if (matchedSegmentCount > 0 && campaignVpaSegmentType == CampaignEnum.Inclusion.ToString())
                        {
                            _logger.LogInformation("{PreFix} Campaign Adding To Qualified Campaign.", prefix);
                            isQualified = true;
                            return isQualified;
                        }
                        else if (matchedSegmentCount <= 0 && campaignVpaSegmentType == CampaignEnum.Exclusion.ToString())
                        {
                            _logger.LogInformation("{PreFix} Campaign Adding To Qualified Campaign.", prefix);
                            isQualified = true;
                            return isQualified;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"error: {ex.ToString()}");
                return false;
            }

            _logger.LogInformation("=========================================================================================================================");
            return isQualified;
        }
        #endregion

        #region Subscription Section
        public SubscriptionModel.Subscription GetSubscriptionById(string preFix, string subscriptionId)
        {
            return _subscriptionService.GetSubscriptionById(preFix, subscriptionId);
        }
        #endregion
    }
}

