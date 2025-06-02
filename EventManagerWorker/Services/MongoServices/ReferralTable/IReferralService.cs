using TransactionModel = Domain.Models.TransactionModel;
using System.Collections.Generic;
using MongoDB.Driver;
using EventManagerWorker.Models;

namespace EventManagerWorker.Services.MongoServices.ReferralTable
{

    public interface IReferralService
    {
        Referral Create(Referral Referral);
        List<Referral> Get();
        List<Referral> Get(FilterDefinition<Referral> filterDefinition);
        Referral Get(string id);
        List<Referral> GetByMobileNumber(string mobileNumber);
        public List<Referral> GetByTransactionId(string ReferralId);
        void Remove(string id);
        void Remove(Referral Referral);
        void Update(string id, Referral Referral);
    }
}
