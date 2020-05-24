using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Outputs
{
    public class ServiceOutput
    {
        public int Id { get; set; }
        public int Company_id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public string Description { get; set; }
        public int Long_time { get; set; }
        public string Address { get; set; }
        public string[] Days { get; set; }
        public string[] Times { get; set; }
        public string[] DaysOff { get; set; }
        public string EndDate { get; set; }
    }
}
