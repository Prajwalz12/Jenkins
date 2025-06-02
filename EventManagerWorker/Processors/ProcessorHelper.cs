using Extensions;
using System;
using Common = Domain.Models.Common;
using TransactionModel = Domain.Models.TransactionModel;
using CustomerModel = Domain.Models.CustomerModel;
using CampaignModel = Domain.Models.CampaignModel;
using System.Collections.Generic;
using Domain.Models.CampaignModel;
using Utility;
using System.Linq;
using Domain.Models.EnumMaster;
using MongoDB.Driver;
using Domain.Models.TransactionModel;
using Domain.Services;
using EventManagerWorker.Utility.Enum;
using Serilog.Events;

namespace Domain.Processors
{
    public static class ProcessorExtension
    {
        public static bool IsRedeemedTransaction(this TransactionModel.Transaction transactionRequest) => typeof(Common.TransactionModel.TransactionDetail).HasProperty("IsRedeem") && transactionRequest.TransactionDetail.IsRedeem;
        public static bool IsLoyaltyFraudConfirmed(this CustomerModel.Customer customer) => customer.Flags?.LoyaltyFraud == 2;
        public static bool IsDirectCampaign(this CampaignModel.EarnCampaign campaign) => string.Equals(campaign.RewardCriteria.RewardIssuance, RewardIssuanceEnum.DIRECT, StringComparison.OrdinalIgnoreCase);

        public static bool IsLockCampaign(this CampaignModel.EarnCampaign campaign, TransactionModel.ProcessedTransaction transaction)
        {
            bool flag = false;
            var rewardIssuance = campaign.RewardCriteria.RewardIssuance;
            var transactionEventCode = EventManager.GetEventCodeByEventName(transaction.TransactionRequest.EventId);
            if (!string.Equals(rewardIssuance, RewardIssuanceEnum.DIRECT, StringComparison.OrdinalIgnoreCase))
            {
                var offerType = campaign.OfferType;
                flag = IsCampaignLocked(campaign, flag, transactionEventCode, offerType, transaction.TransactionRequest.TransactionDetail.Type);
            }
            return flag;

            static bool IsCampaignLocked(CampaignModel.EarnCampaign campaign, bool flag, string transactionEventCode, string offerType, string txnType = null)
            {
                var lockEvent = campaign?.RewardCriteria?.RewardIssuance.ToLower() == RewardIssuanceEnum.WITH_LOCK_UNLOCK ? campaign?.RewardCriteria.WithLockUnlock.LockEvent : null;

                if (lockEvent != null)
                {
                    if (string.Equals(lockEvent.EventType, EventTypeEnum.ACTIVITY, StringComparison.OrdinalIgnoreCase))
                    {
                        return string.Equals(lockEvent?.EventName, transactionEventCode, StringComparison.OrdinalIgnoreCase);
                    }
                    else if (string.Equals(lockEvent.EventType, EventTypeEnum.PAYMENT, StringComparison.OrdinalIgnoreCase))
                    {
                        bool isPayment = false;
                        var rewardCriteria = campaign.RewardCriteria;
                        var isSingle = (rewardCriteria?.WithLockUnlock?.LockEvent?.Event?.GetPaymentDirect().TransactionType == "single");
                        if (isSingle && transactionEventCode.ToLower() == "spend")
                        {
                            isPayment = true;
                        }
                        //if (rewardCriteria.WithLockUnlock?.HybridWithLockUnlock?.LockActivity?.EventName.ToLower() == transactionEventCode.ToLower())

                        return isPayment;
                    }
                    else if (string.Equals(lockEvent.EventType, EventTypeEnum.LENDING, StringComparison.OrdinalIgnoreCase))
                    {
                        return string.Equals(lockEvent?.EventName, transactionEventCode, StringComparison.OrdinalIgnoreCase);
                    }
                }
                return false;
            }
        }

        public static UnlockedCampaignCheckResponse IsUnlockCampaign(this CampaignModel.EarnCampaign campaign, TransactionModel.ProcessedTransaction transaction)
        {
            bool flag = false;
            string offerType = campaign.OfferType;
            string issuanceMode = RewardIssuanceEnum.DIRECT;
            var rewardIssuance = campaign.RewardCriteria.RewardIssuance;
            var transactionEventCode = EventManager.GetEventCodeByEventName(transaction.TransactionRequest.EventId);

            if (!string.Equals(rewardIssuance, RewardIssuanceEnum.DIRECT, StringComparison.OrdinalIgnoreCase))
            {
                issuanceMode = RewardIssuanceEnum.WITH_LOCK_UNLOCK;
                flag = IsCampaignUnlocked(campaign, flag, transactionEventCode, offerType, transaction.TransactionRequest.TransactionDetail.Type);
            }
            return new UnlockedCampaignCheckResponse { IsUnlock = flag, IssuanceMode = issuanceMode, OfferType = offerType };

            static bool IsCampaignUnlocked(CampaignModel.EarnCampaign campaign, bool flag, string transactionEventCode, string offerType, string txnType = null)
            {
                var unlockEvent = campaign?.RewardCriteria?.RewardIssuance.ToLower() == RewardIssuanceEnum.WITH_LOCK_UNLOCK ? campaign?.RewardCriteria.WithLockUnlock.UnlockEvent : null;

                if (unlockEvent != null)
                {

                    if (string.Equals(unlockEvent.EventType, EventTypeEnum.ACTIVITY, StringComparison.OrdinalIgnoreCase))
                    {
                        return string.Equals(unlockEvent?.EventName, transactionEventCode, StringComparison.OrdinalIgnoreCase);
                    }
                    else if (string.Equals(unlockEvent.EventType, EventTypeEnum.PAYMENT, StringComparison.OrdinalIgnoreCase))
                    {
                        bool isPayment = false;
                        var rewardCriteria = campaign.RewardCriteria;
                        var isSingle = (rewardCriteria?.WithLockUnlock?.UnlockEvent?.Event?.GetPaymentDirect().TransactionType == "single");
                        if (isSingle && transactionEventCode.ToLower() == "spend")
                        {
                            isPayment = true;
                        }
                        //if (rewardCriteria.WithLockUnlock?.HybridWithLockUnlock?.LockActivity?.EventName.ToLower() == transactionEventCode.ToLower())

                        return isPayment;
                    }
                    else if (string.Equals(unlockEvent.EventType, EventTypeEnum.LENDING, StringComparison.OrdinalIgnoreCase))
                    {
                        return string.Equals(unlockEvent?.EventName, transactionEventCode, StringComparison.OrdinalIgnoreCase);
                    }
                }
                return false;
            }
        }

        public static IEnumerable<CampaignModel.EarnCampaign> ApplyRMSFilter(this IEnumerable<CampaignModel.EarnCampaign> campaigns, CustomerModel.Customer customer, CustomerModel.CustomerSummary customerSummary)
        {
            IEnumerable<CampaignModel.EarnCampaign> quaifiedCampaigns = new List<CampaignModel.EarnCampaign>();
            foreach (var campaign in campaigns)
            {
                // if rmsattribute doesn't exists in campaign i.e., 
                // its null or empty then
                // qualify the campaign
                // else
                // check if customer has earned some rewards (customer summary is not null)
                // then check if the customer has earned according to all rms attributes (points, cashback)
                // with min and max points

                if (campaign.RmsAttributes == null || campaign.RmsAttributes?.Count <= 0)
                {
                    quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                }
                else
                {
                    // if customer summary exists and rmsattribute in campaign exits
                    if (customerSummary != null)
                    {
                        // campaign.RmsAttributes.Any(o => )
                        const StringComparison _strOrd = StringComparison.OrdinalIgnoreCase;
                        var flag = true;
                        foreach (var attribute in campaign.RmsAttributes)
                        {
                            var pc = attribute.parameterCode;
                            // var ap = attribute.Parameter;
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
                        }

                        if (flag)
                        {
                            quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                        }
                    }
                }
            }
            return quaifiedCampaigns;

        }
        public static (IEnumerable<CampaignModel.EarnCampaign>, IEnumerable<CampaignModel.EarnCampaign>) ApplyMerchantAndBillerFilter(this IEnumerable<CampaignModel.EarnCampaign> campaigns, TransactionModel.ProcessedTransaction transaction)
        {
            const StringComparison _strOrd = StringComparison.OrdinalIgnoreCase;
            if (!string.Equals(transaction.TransactionRequest.EventId, "spend", _strOrd))
            {
                // since only spend with p2p,p2m,and bbps contains merchant or biller 
                // info we don't check further if eventtype is not spend.
                return (campaigns, new List<CampaignModel.EarnCampaign>());
            }
            IEnumerable<CampaignModel.EarnCampaign> quaifiedCampaigns = new List<CampaignModel.EarnCampaign>();
            IEnumerable<CampaignModel.EarnCampaign> quaifiedCumulativeCampaigns = new List<CampaignModel.EarnCampaign>();

            // if the transaction eventtype is spend
            // qualify only campaigns with below conditions
            // offertype=pyament and rewardissuance=direct or
            // offertype=hybrid and rewardissuance=withlockunlock in unlock event section

            var _txnDetail = transaction.TransactionRequest.TransactionDetail;
            foreach (var campaign in campaigns)
            {
                var _offerType = campaign.OfferType;
                var _rewardIssuance = campaign.RewardCriteria.RewardIssuance;

                var _txnPymtCat = _txnDetail.Type;

                var _txnPymtInstrmt = _txnDetail.Payments.Select(o => o.PaymentInstrument);

                if (string.Equals(_offerType, "Payment", _strOrd)
                    && string.Equals(_rewardIssuance, "Direct", _strOrd))
                {
                    var _pymtDirect = campaign?.RewardCriteria?.Direct?.Event?.GetPaymentDirect();

                    // filter out cumulative type campaigns
                    // if (string.Equals(_pymtDirect.TransactionType, "Cumulative", _strOrd))
                    // {
                    //     // add this campaign in cumulative campaign and continue 
                    //     quaifiedCumulativeCampaigns.Append(campaign);
                    //     continue;
                    // }

                    // if campaign is of any type, then add in the qualified campaign and continue
                    if (string.Equals(_pymtDirect.TransactionType, "Any", _strOrd))
                    {
                        // add this campaign in cumulative campaign and continue 
                        quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                        continue;
                    }

                    var _pymtCats = _pymtDirect.PaymentCategories;

                    if (!_pymtCats.Contains(_txnPymtCat, StringComparer.OrdinalIgnoreCase))
                    {
                        // don't qualify campaign if payment category doesn't match
                        continue;
                    }

                    // here we check if any of the transaction paymentinstrument exists in campaign paymentinstrument
                    var _pymtInstrmt = _pymtDirect.PaymentInstruments;
                    bool _txnPymtInstrmtHasMatch = _pymtInstrmt.Select(s => s).Intersect(_txnPymtInstrmt, StringComparer.OrdinalIgnoreCase).Any();
                    if (!_txnPymtInstrmtHasMatch) { continue; }

                    // Now check the paymentcategory for its attribute level validations.
                    // P2P
                    if (string.Equals(_txnPymtCat, "p2p", _strOrd))
                    {

                        if (string.Equals(_pymtDirect.TransactionType, "Cumulative", _strOrd))
                        {
                            // add this campaign in cumulative campaign and continue 
                            quaifiedCumulativeCampaigns = quaifiedCumulativeCampaigns.Append(campaign);
                            continue;
                        }

                        var _txnamt = _txnDetail.Amount;
                        var _mintxnamt = _pymtDirect.Single?.MinTransactionAmount;
                        if (_txnamt >= Convert.ToDouble(_mintxnamt))
                        {
                            quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                            continue;
                        }
                    }

                    // P2M
                    if (string.Equals(_txnPymtCat, "p2m", _strOrd))
                    {
                        var _txnMchntRDlr = _txnDetail.MerchantOrDealer;
                        var _cmpgnMchntRDlr = _pymtDirect.Merchant;

                        if (String.Equals(_cmpgnMchntRDlr.MerchantSegment, "Any", _strOrd))
                        {
                            if (string.Equals(_pymtDirect.TransactionType, "Cumulative", _strOrd))
                            {
                                // add this campaign in cumulative campaign and continue 
                                quaifiedCumulativeCampaigns = quaifiedCumulativeCampaigns.Append(campaign);
                                continue;
                            }
                            quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                            continue;
                        }
                    }

                    // Check for BBPS
                    if (string.Equals(_txnPymtCat, "bbps", _strOrd))
                    {
                        var _cmpgnBllrCats = _pymtDirect.BBPS.BillerCategories;
                        var _txnBllrCat = _txnDetail.Biller;
                        // Updated logic to handle comma-separated Biller IDs
                        bool _containsBillerCat = _cmpgnBllrCats.Any(o =>
                            o.BillerCategory == _txnBllrCat.Category &&
                            o.Biller.Split(',').Select(id => id.Trim()).Contains(_txnBllrCat.Id));
                        if (_containsBillerCat)
                        {
                            if (string.Equals(_pymtDirect.TransactionType, "Cumulative", _strOrd))
                            {
                                // add this campaign in cumulative campaign and continue 
                                quaifiedCumulativeCampaigns = quaifiedCumulativeCampaigns.Append(campaign);
                                continue;
                            }
                            quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                            continue;
                        }
                    }
                }

                //if (string.Equals(_offerType, "Hybrid", _strOrd)
                //    && string.Equals(_rewardIssuance, "withlockunlock", _strOrd))
                //{
                //    var _hybdLkkUnlk = campaign.RewardCriteria.WithLockUnlock.HybridWithLockUnlock;
                //    // filter out cumulative type campaigns
                //    // if (string.Equals(_hybdLkkUnlk.TransactionType, "Cumulative", _strOrd))
                //    // {
                //    //     // add this campaign in cumulative campaign and continue 
                //    //     quaifiedCumulativeCampaigns.Append(campaign);
                //    //     continue;
                //    // }

                //    // if campaign is of any type, then add in the qualified campaign and continue
                //    if (string.Equals(_hybdLkkUnlk.TransactionType, "Any", _strOrd))
                //    {
                //        // add this campaign in cumulative campaign and continue 
                //        quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                //        continue;
                //    }

                //    var _pymtCats = _hybdLkkUnlk.PaymentCategories;

                //    if (!_pymtCats.Contains(_txnPymtCat, StringComparer.OrdinalIgnoreCase))
                //    {
                //        // don't qualify campaign if payment category doesn't match
                //        continue;
                //    }

                //    // here we check if any of the transaction paymentinstrument exists in campaign paymentinstrument
                //    var _pymtInstrmt = _hybdLkkUnlk.PaymentInstruments;
                //    bool _txnPymtInstrmtHasMatch = _pymtInstrmt.Select(s => s).Intersect(_txnPymtInstrmt, StringComparer.OrdinalIgnoreCase).Any();
                //    if (!_txnPymtInstrmtHasMatch) { continue; }

                //    // Now check the paymentcategory for its attribute level validations.
                //    if (string.Equals(_txnPymtCat, "p2p", _strOrd))
                //    {
                //        if (string.Equals(_hybdLkkUnlk.TransactionType, "Cumulative", _strOrd))
                //        {
                //            // add this campaign in cumulative campaign and continue 
                //            quaifiedCumulativeCampaigns = quaifiedCumulativeCampaigns.Append(campaign);
                //            continue;
                //        }
                //        var _txnamt = _txnDetail.Amount;
                //        var _mintxnamt = _hybdLkkUnlk.Single?.MinTransactionAmount;
                //        if (_txnamt >= Convert.ToDouble(_mintxnamt))
                //        {
                //            quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                //            continue;
                //        }
                //    }

                //    //P2M
                //    if (string.Equals(_txnPymtCat, "p2m", _strOrd))
                //    {
                //        var _txnMchntRDlr = _txnDetail.MerchantOrDealer;
                //        var _cmpgnMchntRDlr = _hybdLkkUnlk.Merchant;

                //        if (string.Equals(_cmpgnMchntRDlr.MerchantSegment, "Any", _strOrd))
                //        {
                //            if (string.Equals(_hybdLkkUnlk.TransactionType, "Cumulative", _strOrd))
                //            {
                //                // add this campaign in cumulative campaign and continue 
                //                quaifiedCumulativeCampaigns = quaifiedCumulativeCampaigns.Append(campaign);
                //                continue;
                //            }
                //            quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                //            continue;
                //        }
                //    }

                //    // Check for BBPS
                //    if (string.Equals(_txnPymtCat, "bbps", _strOrd))
                //    {
                //        var _cmpgnBllrCats = _hybdLkkUnlk.BBPS.BillerCategories;
                //        var _txnBllrCat = _txnDetail.Biller;
                //        bool _containsBillerCat = _cmpgnBllrCats.Any(o => o.Biller == _txnBllrCat.Id && o.BillerCategory == _txnBllrCat.Category);
                //        if (_containsBillerCat)
                //        {
                //            if (string.Equals(_hybdLkkUnlk.TransactionType, "Cumulative", _strOrd))
                //            {
                //                // add this campaign in cumulative campaign and continue 
                //                quaifiedCumulativeCampaigns = quaifiedCumulativeCampaigns.Append(campaign);
                //                continue;
                //            }
                //            quaifiedCampaigns = quaifiedCampaigns.Append(campaign);
                //            continue;
                //        }
                //    }
                //}
            }

            return (quaifiedCampaigns, quaifiedCumulativeCampaigns);
            // change parameter to return two object
            // return quaifiedCumulativeCampaigns;
        }

        public static List<CampaignModel.EarnCampaign> ApplyEventTypeCheck(this List<CampaignModel.EarnCampaign> earnCampaigns, TransactionModel.ProcessedTransaction processedTransaction)
        {
            List<CampaignModel.EarnCampaign> qualifiedCampaigns = new List<EarnCampaign>();
            var transactionEventCode = processedTransaction.TransactionRequest.EventId;
            var transactionOfferType = transactionEventCode.GetOfferTypeByEventCode();

            List<CampaignModel.EarnCampaign> campaignsPassedOfferTypeCheck = earnCampaigns.Where(e => e.OfferType == transactionOfferType).ToList();

            if (transactionOfferType == "Activity")
            {
                qualifiedCampaigns.AddRange(campaignsPassedOfferTypeCheck.ApplyEventTypeCheckOnActivityTypeCampaigns(processedTransaction, transactionEventCode, transactionOfferType));
            }
            else if (transactionOfferType == "Payment")
            {
                qualifiedCampaigns.AddRange(campaignsPassedOfferTypeCheck.ApplyEventTypeCheckOnPaymentTypeCampaigns(processedTransaction, transactionEventCode, transactionOfferType));
            }
            else if (transactionOfferType == "Hybrid")
            {
                qualifiedCampaigns.AddRange(campaignsPassedOfferTypeCheck.ApplyEventTypeCheckOnHybridTypeCampaigns(processedTransaction, transactionEventCode, transactionOfferType));
            }

            return qualifiedCampaigns;
        }
        public static List<CampaignModel.EarnCampaign> ApplyEventTypeCheckOnActivityTypeCampaigns(this List<CampaignModel.EarnCampaign> earnCampaigns, TransactionModel.ProcessedTransaction processedTransaction, string transactionEventCode, string transactionOfferType)
        {
            List<CampaignModel.EarnCampaign> campaignsPassedActivityTypeCheck = new List<CampaignModel.EarnCampaign>();
            foreach (var campaign in earnCampaigns)
            {
                var campaignIssuanceMode = campaign.IsDirectCampaign() ? "Direct" : "LockUnlock";
                var rewardCriteria = campaign.RewardCriteria;
                if (campaignIssuanceMode == "Direct")
                {
                    var activityDirect = rewardCriteria.Direct;
                    if (activityDirect != null)
                    {
                        var activityEvent = activityDirect.Event;
                        if (activityEvent != null)
                        {
                            if (activityDirect.EventName == transactionEventCode)
                            {
                                campaignsPassedActivityTypeCheck.Add(campaign);
                            }
                        }
                    }
                }
                else if (campaignIssuanceMode == "LockUnlock")
                {
                    var activityWithLockUnlock = rewardCriteria.WithLockUnlock;
                    if (activityWithLockUnlock != null)
                    {
                        List<EarnCampaign> withLockUnlockCampaigns = new List<EarnCampaign>();
                        List<EarnCampaign> lockUnlockCampaigns = new List<EarnCampaign>();
                        var lockEvent = activityWithLockUnlock.LockEvent;
                        if (lockEvent != null)
                        {
                            if (lockEvent.EventName == transactionEventCode)
                            {
                                lockUnlockCampaigns.Add(campaign);
                            }
                        }
                        var unlockEvent = activityWithLockUnlock.UnlockEvent;
                        if (unlockEvent != null)
                        {
                            if (unlockEvent.EventName == transactionEventCode)
                            {
                                lockUnlockCampaigns.Add(campaign);
                            }
                        }
                        if (lockUnlockCampaigns.Any())
                        {
                            withLockUnlockCampaigns.Add(lockUnlockCampaigns.First());
                        }

                        //var hybridWithLockUnlock = withLockUnlock.HybridWithLockUnlock;
                        //if (hybridWithLockUnlock != null)
                        //{
                        //    List<EarnCampaign> lockUnlockCampaigns = new List<EarnCampaign>();
                        //    var lockEvent = hybridWithLockUnlock.LockActivity;
                        //    if (lockEvent != null)
                        //    {
                        //        if (lockEvent.EventName == transactionEventCode)
                        //        {
                        //            lockUnlockCampaigns.Add(campaign);
                        //        }
                        //    }
                        //    // Need To Check This Unlock Condition;
                        //    lockUnlockCampaigns.Add(campaign);
                        //    if (lockUnlockCampaigns.Any())
                        //    {
                        //        withLockUnlockCampaigns.Add(lockUnlockCampaigns.First());
                        //    }
                        //}
                        if (withLockUnlockCampaigns.Any())
                        {
                            campaignsPassedActivityTypeCheck.Add(withLockUnlockCampaigns.First());
                        }
                    }
                }
            }
            return campaignsPassedActivityTypeCheck;
        }
        public static List<CampaignModel.EarnCampaign> ApplyEventTypeCheckOnPaymentTypeCampaigns(this List<CampaignModel.EarnCampaign> earnCampaigns, TransactionModel.ProcessedTransaction processedTransaction, string transactionEventCode, string transactionOfferType)
        {
            List<CampaignModel.EarnCampaign> campaignsPassedPaymentTypeCheck = new List<CampaignModel.EarnCampaign>();
            foreach (var campaign in earnCampaigns)
            {
                var campaignIssuanceMode = campaign.IsDirectCampaign() ? "Direct" : "LockUnlock";
                if (campaignIssuanceMode == "Direct")
                {
                    campaignsPassedPaymentTypeCheck.Add(campaign);
                }
            }
            return earnCampaigns;
        }
        public static List<CampaignModel.EarnCampaign> ApplyEventTypeCheckOnHybridTypeCampaigns(this List<CampaignModel.EarnCampaign> earnCampaigns, TransactionModel.ProcessedTransaction processedTransaction, string transactionEventCode, string transactionOfferType)
        {
            List<CampaignModel.EarnCampaign> campaignsPassedHybridTypeCheck = new List<CampaignModel.EarnCampaign>();
            foreach (var campaign in earnCampaigns)
            {
                var campaignIssuanceMode = campaign.IsDirectCampaign() ? "Direct" : "LockUnlock";
                var rewardCriteria = campaign.RewardCriteria;
                if (campaignIssuanceMode == "LockUnlock")
                {
                    var withLockUnlock = rewardCriteria.WithLockUnlock;
                    if (withLockUnlock != null)
                    {
                        List<EarnCampaign> withLockUnlockCampaigns = new List<EarnCampaign>();

                        List<EarnCampaign> lockUnlockCampaigns = new List<EarnCampaign>();
                        var lockEvent = withLockUnlock.LockEvent;
                        if (lockEvent != null)
                        {
                            if (lockEvent.EventName == transactionEventCode)
                            {
                                lockUnlockCampaigns.Add(campaign);
                            }
                        }
                        var unlockEvent = withLockUnlock.UnlockEvent;
                        if (unlockEvent != null)
                        {
                            if (unlockEvent.EventName == transactionEventCode)
                            {
                                lockUnlockCampaigns.Add(campaign);
                            }
                        }
                        if (lockUnlockCampaigns.Any())
                        {
                            withLockUnlockCampaigns.Add(lockUnlockCampaigns.First());
                        }

                        //var hybridWithLockUnlock = withLockUnlock.HybridWithLockUnlock;
                        //if (hybridWithLockUnlock != null)
                        //{
                        //    List<EarnCampaign> lockUnlockCampaigns = new List<EarnCampaign>();
                        //    var lockEvent = hybridWithLockUnlock.LockActivity;
                        //    if (lockEvent != null)
                        //    {
                        //        if (lockEvent.EventName == transactionEventCode)
                        //        {
                        //            lockUnlockCampaigns.Add(campaign);
                        //        }
                        //    }
                        //    // Need To Check This Unlock Condition;
                        //    lockUnlockCampaigns.Add(campaign);
                        //    if (lockUnlockCampaigns.Any())
                        //    {
                        //        withLockUnlockCampaigns.Add(lockUnlockCampaigns.First());
                        //    }
                        //}
                        if (withLockUnlockCampaigns.Any())
                        {
                            campaignsPassedHybridTypeCheck.Add(withLockUnlockCampaigns.First());
                        }
                    }
                }
            }
            return earnCampaigns;
        }
        public static IEnumerable<CampaignModel.EarnCampaign> ApplyCustomerStatusFilter(this IEnumerable<CampaignModel.EarnCampaign> earnCampaigns, TransactionModel.ProcessedTransaction processedTransaction, List<DBEnumValue> enumValues)
        {
            IEnumerable<CampaignModel.EarnCampaign> qualifiedCampaigns = new List<EarnCampaign>();
            var customer = processedTransaction.Customer;
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
            return qualifiedCampaigns;
        }

        private static bool ValidateCustomerStatus(string customerFlag, List<string> campaignCustomerStatus, EarnCampaign campaign, CustomerModel.Customer customer)
        {
            var flag = false;
            if (customerFlag == "GDL")
            {
                if (campaignCustomerStatus != null && campaignCustomerStatus.Count > 0 && campaignCustomerStatus.Contains("Delinquency"))
                {
                    flag = true;
                }
            }
            else if (customerFlag == "DRM")
            {
                if (campaignCustomerStatus != null && campaignCustomerStatus.Count > 0 && campaignCustomerStatus.Contains("Dormant"))
                {
                    flag = true;
                }
            }
            else if (customerFlag == "RMS")
            {
                if (campaignCustomerStatus != null && campaignCustomerStatus.Count > 0 && campaignCustomerStatus.Contains("RMS_Loyalty_Fraud"))
                {
                    flag = true;
                }
            }
            else if (customerFlag == "LOB")
            {
                if (customer.Flags.LobFraud.Contains(campaign.LOB))
                {
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
            return flag;
        }

        public static IEnumerable<CampaignModel.EarnCampaign> ApplyInstallationSourceFilter(this IEnumerable<CampaignModel.EarnCampaign> earnCampaigns, CustomerModel.Customer customer)
        {
            IEnumerable<CampaignModel.EarnCampaign> qualifiedCampaigns = new List<EarnCampaign>();
            //var customer = processedTransaction.Customer;
            foreach (var campaign in earnCampaigns)
            {
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
            return qualifiedCampaigns;
        }

    }
}
