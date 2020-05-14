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
        public IActionResult AddService(string name, string description, int price, string address, int long_time, string times, string days)
        {
            try
            {
                string[] daysArray = new string[] { };
                if (!(days is null))
                {
                    daysArray = days.Substring(0, days.Length - 1).Split(';');
                }
                string[] timesArray = times.Substring(0, times.Length - 1).Split(';');
                dbService.AddService(name, price, description, address, dbService.GetCompanyByEmail(User.Identity.Name).Id, daysArray, long_time, timesArray);
                return Json(new { result = "success" });
            } catch (Exception)
            {
                return Json(new { result = "failed" });
            }
        }

        [HttpGet]
        [Route("/Company/UpdateService/{id}")]
        public IActionResult UpdateService(int id)
        {
            ServiceOutput service = dbService.GetServiceById(id);
            ViewBag.Service = service;
            ViewBag.Addresses = dbService.GetCompanyAddressesByEmail(User.Identity.Name);
            return View(ViewBag);
        }

        [HttpPost]
        [Route("/Company/UpdateService/{id}")]
        public IActionResult UpdateService(int id, string name, string description, int price, string address, int long_time, string times, string days)
        {
            try
            {
                string[] daysArray = new string[] { };
                if (!(days is null))
                {
                    daysArray = days.Substring(0, days.Length - 1).Split(';');
                }
                string[] timesArray = times.Substring(0, times.Length - 1).Split(';');
                dbService.UpdateService(id, name, price, description, address, dbService.GetCompanyByEmail(User.Identity.Name).Id, daysArray, long_time, timesArray);
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
            return View(ViewBag);
        }
        ///Files/c1ce5018-cdc4-4de8-9a8d-e574ce3a64bc.jpeg
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

                string[] addressArray = new string[] { };
                if (!(address is null))
                {
                    addressArray = address.Substring(0, address.Length - 1).Split(';');
                }
                Console.WriteLine($"{name} {phone} {email} {description} {filePath}");
                dbService.UpdateCompany(currentCompany.Id, name, phone, email, addressArray, description, filePath);
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