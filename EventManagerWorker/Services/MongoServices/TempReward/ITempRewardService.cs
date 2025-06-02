using TransactionModel = Domain.Models.TransactionModel;
using System.Collections.Generic;
using MongoDB.Driver;
using RewardModel = Domain.Models.RewardModel;

namespace Domain.Services
{
    public interface ITempRewardService
    {
        RewardModel.TempReward Create(RewardModel.TempReward TempReward);
        List<RewardModel.TempReward> Get();
        List<RewardModel.TempReward> Get(FilterDefinition<RewardModel.TempReward> filter);
        RewardModel.TempReward Get(string id);
        List<RewardModel.TempReward> GetByMobileNumber(string mobileNumber);
        public List<RewardModel.TempReward> GetByTransactionId(string transactionId);
        void Remove(string id);
        void Remove(RewardModel.TempReward  TempReward);
        void Update(string id, RewardModel.TempReward  TempReward);
    }
}