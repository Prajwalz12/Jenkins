using TransactionModel = Domain.Models.TransactionModel;
using System.Collections.Generic;
using MongoDB.Driver;
using RewardModel = Domain.Models.RewardModel;

namespace Domain.Services
{
    public interface ITransactionRewardService
    {
        RewardModel.TransactionReward Create(RewardModel.TransactionReward transactionReward);
        List<RewardModel.TransactionReward> Get();
        List<RewardModel.TransactionReward> Get(FilterDefinition<RewardModel.TransactionReward> filter);
        RewardModel.TransactionReward Get(string id);
        List<RewardModel.TransactionReward> GetByMobileNumber(string mobileNumber);
        public List<RewardModel.TransactionReward> GetByTransactionId(string transactionId);
        void Remove(string id);
        void Remove(RewardModel.TransactionReward  transactionReward);
        void Update(string id, RewardModel.TransactionReward  transactionReward);
    }
}