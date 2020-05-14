using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Outputs
{
    public class CompanyOutput
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Description { get; set; }
        public string Pic { get; set; }
        public string[] Address { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
    }
}
