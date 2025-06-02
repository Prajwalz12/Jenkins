using Domain.Models.Common;
using CustomerModel = Domain.Models.CustomerModel;
using Common = Domain.Models.Common;
using TransactionModel = Domain.Models.TransactionModel;
using System.Threading.Tasks;

namespace Domain.Services.ProxyClientServices
{
    public interface IEventManagerService
    {
        Task<bool> UpdateCustomer(CustomerModel.Customer customer, Common.Wallet wallet);
        Task<bool> UpdateTransactionCustomerDetail(TransactionModel.ProcessedTransaction transaction);
    }
}