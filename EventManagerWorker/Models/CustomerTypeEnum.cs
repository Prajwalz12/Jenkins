using System;
using System.Collections.Generic;
using System.Text;

namespace EventManagerWorker.Models
{
    public enum CustomerTypeEnum
    {
        [System.ComponentModel.Description("PTB")] PTB = 0,
        [System.ComponentModel.Description("NTB")] NTB = 1,
        [System.ComponentModel.Description("ETB")] ETB = 2
    }
}
