using Domain.Models;
using MongoDB.Driver;
using System.Collections.Generic;

namespace Domain.Services
{
    public interface IMissedTransactionService
    {
        MissedTransaction Create(MissedTransaction transaction);
        List<MissedTransaction> Get();
        List<MissedTransaction> Get(FilterDefinition<MissedTransaction> filterDefinition);
        MissedTransaction Get(string id);
        List<MissedTransaction> GetByMobileNumber(string mobileNumber);
        List<MissedTransaction> GetByTransactionId(string transactionId);
        void Remove(MissedTransaction transaction);
        void Remove(string id);
        void Update(string id, MissedTransaction transaction);
        void Update(FilterDefinition<MissedTransaction> filterDefinition, UpdateDefinition<MissedTransaction> updateDefinition);
    }
}