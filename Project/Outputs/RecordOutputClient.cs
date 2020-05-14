using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Outputs
{
    public class RecordOutputClient
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string ServiceName { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Address { get; set; }

        public RecordOutputClient(int id, string companyName, string serviceName, string date, string time, string address)
        {
            Id = id;
            CompanyName = companyName;
            ServiceName = serviceName;
            Date = date;
            Time = time;
            Address = address;
        }
    }
}
