using Domain.Models.CustomerModel;
using MongoDB.Driver;
using System.Collections.Generic;

namespace Domain.Services
{
    public interface ICustomerEventService
    {
        CustomerEvent Create(CustomerEvent customer);
        List<CustomerEvent> Get();
        CustomerEvent Get(string id);
        CustomerEvent GetByMobileNumber(string mobileNumber);
        List<CustomerEvent> GetCustomerEvents(FilterDefinition<CustomerEvent> filter);
        long GetCustomerEventsCount(FilterDefinition<CustomerEvent> filter);
        void Remove(CustomerEvent customer);
        void Remove(string id);
        void Update(string id, CustomerEvent customer);
    }
}
