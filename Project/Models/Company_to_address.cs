using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public class Company_to_address
    {
        public int Id { get; set; }
        public int Company_id { get; set; }
        public string Address { get; set; }
    }
}
