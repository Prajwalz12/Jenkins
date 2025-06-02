using Domain.Models;
using MongoDB.Driver;
using System.Collections.Generic;

namespace Domain.Services
{
    public interface IGroupCampaignTransactionService
    {
        GroupedCampaignTransaction Create(GroupedCampaignTransaction transaction);
        List<GroupedCampaignTransaction> Get();
        List<GroupedCampaignTransaction> Get(FilterDefinition<GroupedCampaignTransaction> filterDefinition);
        GroupedCampaignTransaction Get(string id);
        List<GroupedCampaignTransaction> GetByTransactionParentTransactionId(string parentTransactionId);
        void Remove(GroupedCampaignTransaction transaction);
        void Remove(string id);
        void Update(string id, GroupedCampaignTransaction transaction);
    }
}