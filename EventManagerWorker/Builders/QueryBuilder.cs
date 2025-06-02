using Domain.Models.CampaignModel;
using Domain.Processors;
using EventManagerWorker.Models;
using EventManagerWorker.Utility.Enum;
using Extensions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using CampaignModel = Domain.Models.CampaignModel;
using TransactionModel = Domain.Models.TransactionModel;

namespace Domain.Builders
{
    public static class QueryBuilder
    {
        public static FilterDefinition<CampaignModel.EarnCampaign> PrepareFilterQueryWithCollection(this TransactionModel.ProcessedTransaction processedTransaction)
        {
            List<FilterDefinition<CampaignModel.EarnCampaign>> filters = new List<FilterDefinition<CampaignModel.EarnCampaign>>();
            var filterQueryBuilder = Builders<CampaignModel.EarnCampaign>.Filter;
            //filters.Add(PublishedCampaignFilter());
            filters.Add(ActiveCampaignFilter());
            //filters.Add(SuspendedCampaignFilter());
            //filters.Add(LobFilter());//Remove for crossLob function bfl-2653
            filters.Add(ChannelCodeFilter()); // Open When 
            //filters.Add(RewardOptionFilter());
            //filters.Add(TripleRewardFilter());
            //filters.Add(filterQueryBuilder.Where(o=> o.CampaignName == "dfsddfsfsdf" || o.CampaignName == "Voucher4"));
            if (processedTransaction.TransactionRequest.TransactionDetail.DateTime != null)
            {
                filters.Add(StartDateFilter());
                filters.Add(EndDateFilter());
            }

            filters.Add(CustomerTypeFilter());

            if (IsTransactionHasCampaign() && TransactionCampaignRewardFlag() && IsTransactionHasCamgaignId())
            {
                filters.Add(CampaignIdFilter());
            }
            else
            {
                filters.Add(OfferTypeFilter());
            }
            if (!string.IsNullOrEmpty(processedTransaction.TransactionRequest.TransactionDetail.ReferAndEarn?.Referrer) && processedTransaction.TransactionRequest.TransactionDetail.ReferAndEarn.IsReferAndEarn)
            {
                filters.Add(ReferralObjectFilter());
            }

            var filterQueryDefinition = GetCombinedFilterWithAnd(filters);

            return filterQueryDefinition;

            #region Local Function
            //FilterDefinition<CampaignModel.EarnCampaign> CampaignIdNotNullOrEmptyFilter() => filterQueryBuilder.Where(o => !String.IsNullOrEmpty(o.Id));
            //FilterDefinition<CampaignModel.EarnCampaign> CampaignIdFilter() => filterQueryBuilder.Where(o => o.Id == processedTransaction.TransactionRequest.Campaign.Id);
            FilterDefinition<CampaignModel.EarnCampaign> CampaignIdFilter() => filterQueryBuilder.Where(o => o.BFLCampaignId == processedTransaction.TransactionRequest.Campaign.Id);

            FilterDefinition<CampaignModel.EarnCampaign> OfferTypeFilter()
            {
                //List<FilterDefinition<CampaignModel.EarnCampaign>> localFilters = new List<FilterDefinition<CampaignModel.EarnCampaign>>();
                //var localFilterQueryBuilder = Builders<CampaignModel.EarnCampaign>.Filter;

                //if (String.Equals(processedTransaction.TransactionRequest.EventId, "Spend", StringComparison.OrdinalIgnoreCase))
                //{
                //    localFilters.Add(localFilterQueryBuilder.Where(o => o.OfferType == "Hybrid"));
                //    localFilters.Add(localFilterQueryBuilder.Where(o => o.OfferType == "Payment"));
                //    localFilters.Add(localFilterQueryBuilder.Where(o => o.OfferType == "PaymentHybrid"));
                //    // TODO : need to discuss for the change Remove for crossLob function bfl-2653
                //    // if (String.Equals(processedTransaction.TransactionRequest.LOB, "H45-REMUPI", StringComparison.OrdinalIgnoreCase))
                //    //{
                //    if (String.Equals(processedTransaction.TransactionRequest.TransactionDetail.Type, "p2m", StringComparison.OrdinalIgnoreCase))
                //    {
                //        localFilters.Add(localFilterQueryBuilder.Where(o => o.OfferType == "TripleReward"));
                //    }
                //    // }
                //}
                //else
                //{
                //    localFilters.Add(localFilterQueryBuilder.Where(o => o.OfferType == "Activity"));
                //    localFilters.Add(localFilterQueryBuilder.Where(o => o.OfferType == "Hybrid"));
                //    localFilters.Add(localFilterQueryBuilder.Where(o => o.OfferType == "Lending"));
                //}
                //return GetCombinedFilterWithOr(localFilters);
                return filterQueryBuilder.Where(o => o.OfferType == OfferTypeEnum.GENERAL_OFFERS);
            }

            FilterDefinition<CampaignModel.EarnCampaign> EventNameFilter() => filterQueryBuilder.Where(o => o.RewardCriteria.OfferEventDirect == processedTransaction.TransactionRequest.EventId || o.RewardCriteria.OfferEventLock == processedTransaction.TransactionRequest.EventId || o.RewardCriteria.OfferEventUnlock == processedTransaction.TransactionRequest.EventId);

            FilterDefinition<CampaignModel.EarnCampaign> ActiveCampaignFilter() => filterQueryBuilder.Where(o => (o.Status == "ACTIVE" && o.IsPublished == true) || (o.Status == "SUSPENDED" && o.Filter.IsUnlock == true && o.IsPublished == false));
            FilterDefinition<CampaignModel.EarnCampaign> ApprovedCampaignFilter() => filterQueryBuilder.Where(o => o.Status == "APPROVED");
            FilterDefinition<CampaignModel.EarnCampaign> SuspendedCampaignFilter() => filterQueryBuilder.Where(o => o.Status == "SUSPENDED" && o.Filter.IsUnlock == true);

            FilterDefinition<CampaignModel.EarnCampaign> PublishedCampaignFilter() => filterQueryBuilder.Where(o => o.IsPublished);

            //bool IsPaymentInstrumentNullOrEmpty() => String.IsNullOrEmpty(processedTransaction.TransactionRequest.TransactionDetail.PaymentInstrument);
            //Remove for crossLob function bfl-2653
            //FilterDefinition<CampaignModel.EarnCampaign> LobFilter()
            //{
            //    return filterQueryBuilder.Where(o => o.LOB == processedTransaction.TransactionRequest.LOB);
            //}

            bool IsTransactionHasCamgaignId() => !string.IsNullOrEmpty(processedTransaction.TransactionRequest.Campaign.Id);
            bool TransactionCampaignRewardFlag() => processedTransaction.TransactionRequest.Campaign.RewardedFlg;

            bool IsTransactionHasCampaign() => typeof(TransactionModel.Transaction).HasProperty("Campaign") && processedTransaction.TransactionRequest.Campaign != null;
            //bool IsApplyLobFraudFilter() => (typeof(CustomerModel.Customer).HasProperty("Flags") && typeof(Common.CustomerModel.Flag).HasProperty("LobFraud") && processedTransaction.Customer.Flags.LobFraud.Any());
            FilterDefinition<CampaignModel.EarnCampaign> RewardOptionFilter() => filterQueryBuilder.Where(o => o.RewardOption.Count == 1);
            FilterDefinition<CampaignModel.EarnCampaign> CustomerTypeFilter() => filterQueryBuilder.Where(o => o.CustomerType.Contains(processedTransaction.Customer.Type));
            FilterDefinition<CampaignModel.EarnCampaign> StartDateFilter() => filterQueryBuilder.Where(o => o.StartDate <= processedTransaction.TransactionRequest.TransactionDetail.DateTime);
            FilterDefinition<CampaignModel.EarnCampaign> EndDateFilter() => filterQueryBuilder.Where(o => ((o.EndDate != null && o.EndDate >= processedTransaction.TransactionRequest.TransactionDetail.DateTime) || (o.UnLockExpiryDate != null && o.UnLockExpiryDate >= processedTransaction.TransactionRequest.TransactionDetail.DateTime)));
            FilterDefinition<CampaignModel.EarnCampaign> TripleRewardFilter() => filterQueryBuilder.Where(o => o.Filter.IsTripleReward == false);
            FilterDefinition<CampaignModel.EarnCampaign> ChannelCodeFilter() => filterQueryBuilder.Where(o => o.Channel.Contains(processedTransaction.TransactionRequest.ChannelCode));
            FilterDefinition<CampaignModel.EarnCampaign> StaticDateFilterFilter() => filterQueryBuilder.Where(o => o.RewardOption[0].RewardType == "promovoucher" && o.RewardOption[0].PromoVoucherDetails.StaticValidDate > DateTime.Now);
            FilterDefinition<CampaignModel.EarnCampaign> ReferralObjectFilter() => filterQueryBuilder.Where(o => o.Filter.IsRefferalProgram == true);
            #endregion
        }

        private static FilterDefinition<EarnCampaign> GetCombinedFilterWithAnd(List<FilterDefinition<EarnCampaign>> filters)
        {
            var combinedFilter = filters[0];
            if (filters.Count == 1)
            {
                return filters[0];
            }
            for (int i = 1; i < filters.Count; i++)
            {
                combinedFilter &= filters[i];
            }
            return combinedFilter;
        }
        private static FilterDefinition<EarnCampaign> GetCombinedFilterWithOr(List<FilterDefinition<EarnCampaign>> filters)
        {
            var combinedFilter = filters[0];
            if (filters.Count == 1)
            {
                return filters[0];
            }
            for (int i = 1; i < filters.Count; i++)
            {
                combinedFilter |= filters[i];
            }
            return combinedFilter;
        }

        public static FilterDefinition<MobileCampaignForOncePerDestination> PrepareFilterQuery(MobileCampaignOncePerDestinationRequest request)
        {
            var filterBuilder = Builders<MobileCampaignForOncePerDestination>.Filter;
            List<FilterDefinition<MobileCampaignForOncePerDestination>> filterCollection = new List<FilterDefinition<MobileCampaignForOncePerDestination>>();

            if (!String.IsNullOrEmpty(request.MobileNumber) && (!String.IsNullOrEmpty(request.CampaignId)))
            {
                filterCollection.Add(filterBuilder.Where(o => o.MobileNumber == request.MobileNumber && o.CampaignId == request.CampaignId));
            }
            if (!String.IsNullOrEmpty(request.DestinationVPA))
            {
                filterCollection.Add(filterBuilder.Where(o => o.DestinationVPA == request.DestinationVPA));
            }
            else if (!String.IsNullOrEmpty(request.MerchantId))
            {
                filterCollection.Add(filterBuilder.Where(o => o.MerchantId == request.MerchantId));
            }
            else if (!String.IsNullOrEmpty(request.BillerId))
            {
                filterCollection.Add(filterBuilder.Where(o => o.BillerId == request.BillerId));
            }
            return GetCombinedFilterWithAnd<MobileCampaignForOncePerDestination>(filterCollection);
        }

        private static FilterDefinition<T> GetCombinedFilterWithAnd<T>(List<FilterDefinition<T>> filters)
        {
            var combinedFilter = filters[0];
            if (filters.Count == 1)
            {
                return filters[0];
            }
            for (int i = 1; i < filters.Count; i++)
            {
                combinedFilter &= filters[i];
            }
            return combinedFilter;
        }
    }
}
