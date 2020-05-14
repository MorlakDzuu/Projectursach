using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public class Service
    {
        public int Id { get; set; }
        public int Company_id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public string Description { get; set; }
        public int Long_time { get; set; }
        public int Address_id { get; set; }
    }
}
