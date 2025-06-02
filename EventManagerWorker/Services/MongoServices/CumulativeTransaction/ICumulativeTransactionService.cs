using Domain.Models.TransactionModel;
using System.Collections.Generic;

namespace Domain.Services
{
    public interface ICumulativeTransactionService
    {
        ProcessedTransaction Create(ProcessedTransaction transaction);
        List<ProcessedTransaction> Get();
        ProcessedTransaction Get(string id);
        List<ProcessedTransaction> GetByMobileNumber(string mobileNumber);
        List<ProcessedTransaction> GetByTransactionId(string transactionId);
        void Remove(string id);
        void Remove(ProcessedTransaction transaction);
        void Update(string id, ProcessedTransaction transaction);
    }
}