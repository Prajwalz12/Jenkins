using CustomerModel = Domain.Models.CustomerModel;
using System.Collections.Generic;

namespace Domain.Services
{
    public interface ICustomerSummaryService
    {
        CustomerModel.CustomerSummary Create(CustomerModel.CustomerSummary customer);
        List<CustomerModel.CustomerSummary> Get();
        CustomerModel.CustomerSummary Get(string id);
        CustomerModel.CustomerSummary GetByMobileNumber(string mobileNumber);
        CustomerModel.Customer GetCustomerByMobileNumber(string mobileNumber);
        void Remove(CustomerModel.CustomerSummary customer);
        void Remove(string id);
        void Update(string id, CustomerModel.CustomerSummary customer);
    }
}