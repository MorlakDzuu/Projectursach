using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public class Record
    {
        public int Id { get; set; }
        public int User_id { get; set; }
        public int Service_id { get; set; }
        public int Time_id { get; set; }
        public string Status { get; set; }
        public string Date { get; set; }
    }
}
