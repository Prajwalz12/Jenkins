using CampaignModel = Domain.Models.CampaignModel;
using TransactionModel = Domain.Models.TransactionModel;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;
using Domain.Models.CampaignModel;
using MongoDB.Driver;

namespace Domain.Services
{
    public interface ICampaignService
    {
        CampaignModel.EarnCampaign Create(CampaignModel.EarnCampaign campaign);
        List<CampaignModel.EarnCampaign> Get();
        CampaignModel.EarnCampaign Get(string id);
        void Remove(CampaignModel.EarnCampaign campaign);
        void Remove(string id);
        void Update(string id, CampaignModel.EarnCampaign campaign);
        //List<CampaignModel.EarnCampaign> GetCampaignsUsingFilter(TransactionModel.Transaction transaction);
        List<CampaignModel.EarnCampaign> GetCampaignsUsingFilter(Expression<Func<CampaignModel.EarnCampaign, bool>> expression);
        List<EarnCampaign> GetCampaignsUsingFilter(FilterDefinition<EarnCampaign> filter);
    }
}