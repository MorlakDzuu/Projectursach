using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Project.Services;
using System;
using Project.Models;
using Project.Outputs;

namespace Project
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
