using TransactionModel = Domain.Models.TransactionModel;
using System.Collections.Generic;
using MongoDB.Driver;
using System.Transactions;

namespace Domain.Services
{
    public interface ITransactionService
    {
        TransactionModel.Transaction Create(TransactionModel.Transaction transaction);
        List<TransactionModel.Transaction> Get();
        List<TransactionModel.Transaction> Get(FilterDefinition<TransactionModel.Transaction> filterDefinition);
        TransactionModel.Transaction Get(string id);
        List<TransactionModel.Transaction> GetByMobileNumber(string mobileNumber);
        public List<TransactionModel.Transaction> GetByTransactionId(string transactionId);
        void Remove(string id);
        void Remove(TransactionModel.Transaction transaction);
        void Update(string id, TransactionModel.Transaction transaction);
    }
}
