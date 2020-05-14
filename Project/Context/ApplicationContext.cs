using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Project.Models;

namespace Project.Context
{
    public class ApplicationContext: DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Record> Records { get; set; }
        public DbSet<Company_to_address> Company_to_address { get; set; }
        public DbSet<Service_to_day> Service_to_day { get; set; }
        public DbSet<Service_to_time> Service_to_time { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=project;Username=postgres;Password=42657238");
        }
    }
}
