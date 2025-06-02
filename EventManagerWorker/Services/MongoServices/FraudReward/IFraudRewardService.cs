using Domain.Services;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public interface IFraudRewardService 
    {
        List<Domain.Models.RewardModel.FraudReward> Get(FilterDefinition<Domain.Models.RewardModel.FraudReward> filter);
    }
}
