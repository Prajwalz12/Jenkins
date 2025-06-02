using CustomerModel = Domain.Models.CustomerModel;
using System.Collections.Generic;

namespace Domain.Services
{
    public interface ICustomerVersionService
    {
        CustomerModel.CustomerVersion Create(CustomerModel.CustomerVersion customerVersion);
        List<CustomerModel.CustomerVersion> Get();
        CustomerModel.CustomerVersion Get(string id);
        CustomerModel.CustomerVersion GetByMobileNumber(string mobileNumber);
        CustomerModel.CustomerVersion GetLatestCustomerVersion(string mobileNumber);
        //void Remove(CustomerVersion customerVersion);
        //void Remove(string id);
        //void Update(string id, CustomerVersion customerVersion);
    }
}