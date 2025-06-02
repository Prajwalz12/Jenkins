using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models
{
    public class LapsePolicy
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }    
        public string DurationType { get; set; }
        public int DurationValue { get; set; }        
    }
}
