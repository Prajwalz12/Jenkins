using Domain.Models.TransactionModel;
using System.Collections.Generic;

namespace Domain.Services
{
    public interface IOfferMapService
    {
        ProcessedTransaction Create(ProcessedTransaction transaction);
        List<ProcessedTransaction> Get();
        ProcessedTransaction Get(string id);
        List<ProcessedTransaction> GetByMobileNumber(string mobileNumber);
        List<ProcessedTransaction> GetByTransactionId(string transactionId);
        void Remove(ProcessedTransaction transaction);
        void Remove(string id);
        void Update(string id, ProcessedTransaction transaction);
    }
}