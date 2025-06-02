using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models
{
    public static class DomainEvent
    {
        public enum EventType
        {
            WalletCreation,
            WalletLoad,
            Spend,
            VPACreation,
            ClearBounceEMI,
            KYCCompletion,
            Signup,
            CustomRewarding,
            CDFEMI
        }
    }
}
