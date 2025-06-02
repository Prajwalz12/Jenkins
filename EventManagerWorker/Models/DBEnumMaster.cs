using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Models.EnumMaster
{
    public class DBEnumMaster
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
    public class DBEnumValue
    {
        public int MasterId { get; set; }
        public string MasterCode { get; set; }
        public string MasterName { get; set; }

        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public string GroupMerchantCode { get; set; } = null; // MerchantGroupCode
        public string GroupMerchantName { get; set; } = null;
        public string BrandCode { get; set; } = null;
    }

    //public class DBEnumValue
    //{
    //    public int MasterId { get; set; }
    //    public string MasterCode { get; set; }
    //    public string MasterName { get; set; }

    //    public int ValueId { get; set; }
    //    public string ValueCode { get; set; }
    //    public string ValueName { get; set; }

    //    public int ValueId { get; set; }
    //    public string ValueCode { get; set; }
    //    public string ValueName { get; set; }
    //}

}
