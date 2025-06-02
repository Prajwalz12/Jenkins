using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Services
{
    public interface ISubscriptionService
    {
        Task<List<Domain.Models.SubscriptionModel.Subscription>> GetEligibleSubscriptionsAsync();
        Task<Domain.Models.SubscriptionModel.Subscription> GetSubscriptionAsync(string subscriptionId);
        Domain.Models.SubscriptionModel.Subscription GetSubscriptionById(string preFix, string subscriptionId);
    }
}