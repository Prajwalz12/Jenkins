using DBService;
using EventManagerWorker.Models;
using System.Collections.Generic;

namespace Domain.Services
{
    public interface IMerchantMaster
    {
        MerchantMaster GetMerchantMasterValues(MerchantEnumRequest request);
    }
}
