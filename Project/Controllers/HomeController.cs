using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Project.Models;
using Project.Services;
using Project.Outputs;
using Microsoft.VisualBasic.CompilerServices;

namespace Project.Controllers
{
    public class HomeController : Controller
    {
        DbService dbService = new DbService();

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Companies()
        {
            return View();
        }

        [HttpGet]
        [Route("/Home/CompanyPage/{id}")]
        public IActionResult CompanyPage(int id)
        {
            ViewBag.Company = dbService.GetCompanyById(id);
            ViewBag.Services = dbService.GetServicesByCompanyId(id);
            return View(ViewBag);
        }

        [HttpGet]
        [Route("/Home/MakeRecord/{id}")]
        public IActionResult MakeRecord(int id)
        {
            ViewBag.Service = dbService.GetServiceById(id);
            return View(ViewBag);
        }

        [HttpPost]
        [Route("/Home/GetTimes/{id}")]
        public IActionResult GetTimes(int id, string date)
        {
            try
            {
                if (!(date is null))
                {
                    ServiceOutput service = dbService.GetServiceById(id);
                    DateTime dateDT = DateTime.Parse(date);
                    if ((dateDT.CompareTo(DateTime.Now) > 0) || (dateDT.Equals(DateTime.Now.Date)))
                    {
                        if (service.Days.ToList().Contains(dateDT.DayOfWeek.ToString()) && (!service.DaysOff.ToList().Contains(DateTime.Parse(date).ToString("dd-MM-yyyy"))) && (DateTime.Parse(service.EndDate).CompareTo(dateDT) > 0))
                        {
                            return Json(new
                            {
                                result = "success",
                                times = dbService.GetTimesByDateAndServiceId(dateDT.Date.ToString(), id)
                            });
                        }
                    }
                }
                return Json(new { result = "failed" });
            }
            catch (Exception)
            {
                return Json(new { result = "failed" });
            }
        }

        [HttpPost]
        public IActionResult AddRecord(string name, string email, string phone, string date, string time, int service_id)
        {
            ServiceOutput service = dbService.GetServiceById(service_id);
            string address = service.Address;
            int recordId;
            try
            {
                List<User> users = dbService.GetUsersByEmailAndPhone(email, phone);
                if (users.Count > 0)
                {
                    recordId = dbService.AddRecord(users[0].Id, service_id, address, time, date);
                }
                else
                {
                    int id = dbService.AddUser(name, email, phone);
                    recordId = dbService.AddRecord(id, service_id, address, time, date);
                }
                Console.WriteLine(recordId);
                return Json(new { result = "success", record_id = recordId.ToString() });
            }
            catch (Exception)
            {
                return Json(new { result = "failed" });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        [Route("/Home/ClientRecords/{ids}")]
        public IActionResult ClientRecords(string ids)
        {
            Console.WriteLine(ids);
            List<RecordOutputClient> records = new List<RecordOutputClient> { };
            ids.Trim().Split(' ').ToList().ForEach(id =>
            {
                records.Add(dbService.GetRecordOutputByRecordId(int.Parse(id)));
            });
            ViewBag.Records = records;
            return View();
        }

        [HttpPost]
        [Route("/Home/DeleteRecord/{id}")]
        public IActionResult DeleteRecord(int id)
        {
            try
            {
                Record record = dbService.GetRecordById(id);
                record.Status = "canceled";
                dbService.UpdateRecord(record);
                return Json(new { result = "success" });
            }
            catch (Exception)
            {
                return Json(new { result = "failed" });

            }
        }
    }
}