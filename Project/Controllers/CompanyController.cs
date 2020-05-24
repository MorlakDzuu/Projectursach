using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.Services;
using Project.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Security.Cryptography;
using Project.Outputs;
using Newtonsoft.Json;
using Project.Json;
using Newtonsoft.Json.Linq;
using Project.Inputs;

namespace Project.Controllers
{
    [Authorize(Roles = "company", AuthenticationSchemes = "Bearer")]
    public class CompanyController : Controller
    {
        DbService dbService = new DbService();
        IWebHostEnvironment appEnvironment;

        public CompanyController(IWebHostEnvironment appEnvironment)
        {
            this.appEnvironment = appEnvironment;
        }

        [HttpGet]
        public IActionResult CompanyPage()
        {
            ViewBag.Company = dbService.GetCompanyByEmail(User.Identity.Name);
            ViewBag.Services = dbService.GetServicesByCompanyId(ViewBag.Company.Id);
            return View(ViewBag);
        }

        [HttpGet]
        public IActionResult AddService()
        {
            ViewBag.Addresses = dbService.GetCompanyAddressesByEmail(User.Identity.Name);
            return View(ViewBag);
        }

        [HttpPost]
        public IActionResult AddService(string name, string description, int price, string address, int long_time, string days_times, string end_date, string days_off)
        {
            try
            {
                List<DayTimes> daysTimes = JObject.Parse(days_times)["daysTimes"].ToObject<DayTimes[]>().ToList();
                dbService.AddService(name, price, description, address, dbService.GetCompanyByEmail(User.Identity.Name).Id, daysTimes, long_time, end_date, days_off.Split(',').ToList());
                return Json(new { result = "success" });
            } catch (Exception)
            {
                return Json(new { result = "failed" });
            }
        }

        public static List<string> GetTimesByDay(List<DayTimes> dayTimes , string day)
        {
            List<DayTimes> dayTimesByDay = dayTimes.Where(item => item.Day.Equals(day)).ToList();
            if (dayTimesByDay.Count > 0)
            {
                return dayTimesByDay[0].Times;
            }
            return new List<string> { };
        }

        [HttpGet]
        [Route("/Company/UpdateService/{id}")]
        public IActionResult UpdateService(int id)
        {
            ServiceOutput service = dbService.GetServiceById(id);
            ViewBag.Service = service;
            ViewBag.Addresses = dbService.GetCompanyAddressesByEmail(User.Identity.Name);
            ViewBag.DaysTimes = dbService.GetDaysTimesByServiceId(id);
            string datesOff = "";
            foreach (DayOff dayOff in dbService.GetDaysOffByServiceId(id))
            {
                datesOff += dayOff.Date + ',';
            }
            ViewBag.DatesOff = datesOff.Substring(0, datesOff.Length - 1);
            return View(ViewBag);
        }

        [HttpPost]
        [Route("/Company/UpdateService/{id}")]
        public IActionResult UpdateService(int id, string name, string description, int price, string address, int long_time, string days_times, string end_date, string days_off)
        {
            try
            {
                List<DayTimes> daysTimes = JObject.Parse(days_times)["daysTimes"].ToObject<DayTimes[]>().ToList();
                dbService.UpdateService(id, name, price, description, address, dbService.GetCompanyByEmail(User.Identity.Name).Id, daysTimes, long_time, end_date, days_off.Split(',').ToList());
                return Json(new { result = "success" });
            }
            catch (Exception)
            {
                return Json(new { result = "failed" });
            }
        }

        [HttpGet]
        public IActionResult UpdateCompany()
        {
            ViewBag.Company = dbService.GetCompanyByEmail(User.Identity.Name);
            ViewBag.Addresses = dbService.GetAddressesByCompanyId(dbService.GetCompanyByEmail(User.Identity.Name).Id);
            return View(ViewBag);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCompany(string name, string phone, string email, string address, string description, IFormFile pic)
        {
            CompanyOutput currentCompany = dbService.GetCompanyByEmail(User.Identity.Name);
            if (dbService.GetCompanyByEmail(email) is null || (currentCompany.Email.Equals(email)))
            {
                string filePath = String.Empty;
                if (!(pic is null))
                {
                    filePath = "/Files/" + Guid.NewGuid() + "." + pic.FileName.Split('.')[1];
                    using (var fileStream = new FileStream(appEnvironment.WebRootPath + filePath, FileMode.Create))
                    {
                        await pic.CopyToAsync(fileStream);
                    }
                } else
                {
                    filePath = currentCompany.Pic;
                }
                List<Address> addresses = JObject.Parse(address)["addresses"].ToObject<Address[]>().ToList();
                dbService.UpdateCompany(currentCompany.Id, name, phone, email, addresses, description, filePath);
                return Json(new { Result = "success" });
            }
            return Json(new { Result = "failed" });
        }

        [HttpGet]
        public IActionResult Records()
        {
            ViewBag.Records = dbService.GetRecordsByCompanyId(dbService.GetCompanyByEmail(User.Identity.Name).Id);
            ViewBag.Number = 1;
            return View(ViewBag);
        }

        [HttpGet]
        public IActionResult RecordsToday()
        {
            ViewBag.Records = dbService.GetRecordsByCompanyIdAndDate(dbService.GetCompanyByEmail(User.Identity.Name).Id, DateTime.Now);
            ViewBag.Number = 2;
            return View("Records");
        }

        [HttpGet]
        public IActionResult RecordsTomorrow()
        {
            ViewBag.Records = dbService.GetRecordsByCompanyIdAndDate(dbService.GetCompanyByEmail(User.Identity.Name).Id, DateTime.Now.AddDays(1));
            ViewBag.Number = 3;
            return View("Records");
        }

        [HttpGet]
        [Route("/Company/RecordsSearch/{search}")]
        public IActionResult RecordsSearch(string search)
        {
            ViewBag.Records = dbService.GetRecordsBySearchAndCompanyId(dbService.GetCompanyByEmail(User.Identity.Name).Id, search);
            ViewBag.Number = 1;
            return View("Records");
        }
    }
}