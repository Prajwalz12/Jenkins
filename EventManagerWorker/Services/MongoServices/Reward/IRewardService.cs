using TransactionModel = Domain.Models.TransactionModel;
using System.Collections.Generic;
using MongoDB.Driver;
using RewardModel = Domain.Models.RewardModel;

namespace Domain.Services
{
    public interface IRewardService
    {
        //RewardModel.Reward Create(RewardModel.Reward transactionReward);
        //List<RewardModel.Reward> Get();
        List<RewardModel.TransactionReward> Get(string collectionName, FilterDefinition<RewardModel.TransactionReward> filter);
        //RewardModel.Reward Get(string id);
        //List<RewardModel.Reward> GetByMobileNumber(string mobileNumber);
        //public List<RewardModel.Reward> GetByTransactionId(string transactionId);
        //void Remove(string id);
        //void Remove(RewardModel.TransactionReward  transactionReward);
        //void Update(string id, RewardModel.TransactionReward  transactionReward);
    }
}