using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Outputs
{
    public class RecordOutput
    {
        public int Id { get; set; }
        public int User_id { get; set; }
        public int Service_id { get; set; }
        public string Time { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }
        public string Date { get; set; }
    }
}
