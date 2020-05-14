using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Outputs
{
    public class RecordOutputAdmin
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Time { get; set; }
        public string Address { get; set; }
        public string RecordName { get; set; }  
        public string Date { get; set; }

        public RecordOutputAdmin(string name, string email, string phone, string time, string address, string recordName, string date)
        {
            Name = name;
            Email = email;
            Phone = phone;
            Time = time;
            Address = address;
            RecordName = recordName;
            Date = date;
        }
    }
}
