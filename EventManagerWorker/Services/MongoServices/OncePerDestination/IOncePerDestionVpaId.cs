using EventManagerWorker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagerWorker.Services.MongoServices.OncePerDestination
{
    public interface IOncePerDestionVpaId
    {
        // MobileCampaignForOncePerDestination 
        MobileCampaignForOncePerDestination GetMobileCampaignDestination(MobileCampaignOncePerDestinationRequest request);

        long GetTransactionCountForOncePerPayee(string mobileNumber, string campId);
    }
}
