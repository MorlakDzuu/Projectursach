using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Project.Models;
using Project.Context;
using System.Security.Cryptography;
using Project.Outputs;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Project.Json;
using Project.Inputs;

namespace Project.Services
{
    public class DbService
    {
        ApplicationContext db;

        public DbService()
        {
            db = new ApplicationContext();
        }


        // User queries
        public int AddUser(string name, string email, string phone)
        {
            User user = new User { Name = name, Email = email, Phone = phone };
            db.Users.Add(user);
            db.SaveChanges();
            return GetUsersByEmailAndPhone(email, phone)[0].Id;
        }

        public User GetUserById(int id)
        {
            return db.Users.Find(id);
        }

        public List<User> GetUsersByEmailAndPhone(string email, string phone)
        {
            return db.Users.Where(u => u.Email.Equals(email) && u.Phone.Equals(phone)).ToList();
        }

        /// <summary>
        /// Возвращает определенное количество объектов Пользователей.
        /// </summary>
        /// <param name="from">Индекс начала range</param>
        /// <param name="to">Индекс конца range</param>
        /// <returns>Коллекция объектов User</returns>
        public List<User> GetUsersRange(int from, int to)
        {
            from--;
            to--;
            try
            {
                return db.Users.ToList().GetRange(from, to - from);
            } catch(Exception)
            {
                return new List<User> { };
            }
        }

        // Company queries
        public void AddCompany(string name, string phone, string email, string[] address, string description, string pic, string passwordHash, string role)
        {
            Company company = new Company { Name = name, Phone = phone, Email = email, Description = description, Pic = pic, PasswordHash = passwordHash, Role = role };
            db.Companies.Add(company);
            db.SaveChanges();
            int id = GetCompanyByEmail(email).Id;
            foreach (string item in address.ToList())
            {
                db.Company_to_address.Add(new Company_to_address { Company_id = id, Address = item });
            }
            db.SaveChanges();
        }

        public CompanyOutput GetCompanyById(int id)
        {
            Company company = db.Companies.Find(id);
            List<string> addresses = db.Company_to_address.Where(item => item.Company_id == company.Id).Select(item => item.Address).ToList();
            CompanyOutput companyOutput = new CompanyOutput
            {
                Id = company.Id,
                Name = company.Name,
                Email = company.Email,
                Phone = company.Phone,
                Description = company.Description,
                Pic = company.Pic,
                PasswordHash = company.PasswordHash,
                Role = company.Role,
                Address = addresses.ToArray()
            };
            return companyOutput;
        }

        public CompanyOutput GetCompanyByEmail(string email)
        {
            Company company = db.Companies.FirstOrDefault(c => c.Email == email);
            if (company is null)
            {
                return null;
            } else
            {
                return GetCompanyById(company.Id);
            }
        }

        public List<Company_to_address> GetAddressesByCompanyId(int id)
        {
            return db.Company_to_address.Where(item => item.Company_id == id).ToList();
        }

        public void UpdateCompany(int id, string name, string phone, string email, List<Address> addresses, string description, string pic)
        {
            CompanyOutput oldCompanyOut = GetCompanyById(id);
            Company oldCompany = db.Companies.Find(id);
            Company company = new Company { Id = id, Name = name, Phone = phone, Email = email, Description = description, Pic = pic, PasswordHash = oldCompanyOut.PasswordHash, Role = oldCompanyOut.Role };
            //List<string> addresses = db.Company_to_address.Where(item => item.Company_id == id).Select(item => item.Address).ToList();
            db.Entry(oldCompany).State = EntityState.Detached;
            db.Companies.Update(company);
            db.SaveChanges();
            foreach (Address item in addresses)
            {
                if (item.Id != 0)
                {
                    db.Company_to_address.Update(new Company_to_address { Id = item.Id, Address = item.Value, Company_id = id});
                }
                else
                {
                    db.Company_to_address.Add(new Company_to_address { Company_id = id, Address = item.Value });
                }
            }
            db.SaveChanges();
        }

        public List<string> GetCompanyAddressesByEmail(string email)
        {
            CompanyOutput company = GetCompanyByEmail(email);
            return company.Address.ToList();
        }

        public List<CompanyOutput> GetCompanies()
        {
            List<Company> companies = db.Companies.ToList();
            List<CompanyOutput> companyOutputs = new List<CompanyOutput> { };
            foreach (Company company in companies)
            {
                CompanyOutput companyOutput = new CompanyOutput
                {
                    Id = company.Id,
                    Name = company.Name,
                    Email = company.Email,
                    Phone = company.Phone,
                    Description = company.Description,
                    Pic = company.Pic,
                    PasswordHash = company.PasswordHash,
                    Role = company.Role,
                    Address = db.Company_to_address.Where(item => item.Company_id == company.Id).Select(item => item.Address).ToArray()
                };
                companyOutputs.Add(companyOutput);
            }
            return companyOutputs;
        }

        /// <summary>
        /// Возвращает определенное количество объектов Компаний.
        /// </summary>
        /// <param name="from">Индекс начала range</param>
        /// <param name="to">Индекс конца range</param>
        /// <returns>Коллекция объектов Company</returns>
        public List<CompanyOutput> GetCompaniesRange(int from, int to)
        {
            from--;
            to--;
            try
            {
                List<Company> companies = db.Companies.ToList().GetRange(from, to - from);
                List<CompanyOutput> companyOutputs = new List<CompanyOutput> { };
                foreach (Company company in companies)
                {
                    CompanyOutput companyOutput = new CompanyOutput
                    {
                        Id = company.Id,
                        Name = company.Name,
                        Email = company.Email,
                        Phone = company.Phone,
                        Description = company.Description,
                        Pic = company.Pic,
                        PasswordHash = company.PasswordHash,
                        Role = company.Role,
                        Address = db.Company_to_address.Where(item => item.Company_id == company.Id).Select(item => item.Address).ToArray()
                    };
                    companyOutputs.Add(companyOutput);
                }
                return companyOutputs;
            }
            catch (Exception)
            {
                return new List<CompanyOutput> { };
            }
        }


        // Service queries
        public void AddService(string name, int price, string description, string address, int company_id, List<DayTimes> daysTimes, int long_time, string endDate, List<string> datesOff)
        {
            int addressId = db.Company_to_address.Where(item => item.Company_id == company_id & item.Address.Equals(address)).ToList()[0].Id;
            Service service = new Service { 
                Name = name, 
                Price = price, 
                Description = description, 
                Company_id = company_id, 
                Long_time = long_time, 
                Address_id = addressId,
                EndDate = endDate
            };
            db.Services.Add(service);
            db.SaveChanges();
            int serviceId = db.Services.Where(item => item.Name.Equals(name) && item.Price == price && item.Description.Equals(description) && item.Company_id == company_id && item.Long_time == long_time && item.Address_id == addressId).ToList()[0].Id;
            foreach (DayTimes dayTimes in daysTimes)
            {
                foreach (String time in dayTimes.Times)
                {
                    db.Service_to_time.Add(new Service_to_time { Service_id = serviceId, Day = dayTimes.Day, Time = time, Status = "new" });
                }
            }
            foreach (string date in datesOff)
            {
                db.Days_off.Add(new DayOff { Service_id = serviceId, Date = date });
            }
            db.SaveChanges();
        }

        public void UpdateService(int id, string name, int price, string description, string address, int company_id, List<DayTimes> daysTimes, int long_time, string endDate, List<string> datesOff)
        {
            Service service = new Service
            {
                Id = id,
                Name = name,
                Price = price,
                Description = description,
                Company_id = company_id,
                Long_time = long_time,
                Address_id = db.Company_to_address.Where(item => item.Company_id == company_id & item.Address.Equals(address)).ToList()[0].Id,
                EndDate = endDate
            };
            db.Services.Update(service);
            db.SaveChanges();
            List<string> oldDatesOff = db.Days_off.Where(item => item.Service_id == id).Select(item => item.Date).ToList();
            foreach (string date in datesOff)
            {
                if (!oldDatesOff.Contains(date))
                {
                    db.Days_off.Add(new DayOff { Service_id = id, Date = date });
                } else
                {
                    oldDatesOff.Remove(date);
                }
            }
            foreach (string date in oldDatesOff)
            {
                db.Days_off.Remove(db.Days_off.Where(item => item.Service_id == id && item.Date.Equals(date)).ToList()[0]);
            }

            List<DayTimes> dayTimesLast = daysTimes.ToList();
            foreach (DayTimes dayTimes in daysTimes)
            {
                List<Service_to_time> servicesByDay = db.Service_to_time.Where(item => item.Service_id == id && item.Day.Equals(dayTimes.Day)).ToList();
                foreach (Service_to_time service_To_Time in servicesByDay)
                {
                    if (dayTimes.Times.Contains(service_To_Time.Time))
                    {
                        if (service_To_Time.Status.Equals("old"))
                        {
                            db.Entry(service_To_Time).State = EntityState.Detached;
                            db.Service_to_time.Update(new Service_to_time { Id = service_To_Time.Id, Service_id = id, Day = service_To_Time.Day, Time = service_To_Time.Time, Status = "new" });
                        }
                        dayTimesLast[daysTimes.IndexOf(dayTimes)].Times.Remove(service_To_Time.Time);
                    } else
                    {
                        db.Entry(service_To_Time).State = EntityState.Detached;
                        db.Service_to_time.Update(new Service_to_time { Id = service_To_Time.Id, Service_id = id, Day = service_To_Time.Day, Time = service_To_Time.Time, Status = "old" });
                    }
                }
                foreach (string time in dayTimesLast[daysTimes.IndexOf(dayTimes)].Times)
                {
                    db.Service_to_time.Add(new Service_to_time { Service_id = id, Day = dayTimes.Day, Time = time, Status = "new" });
                }
            }
            db.SaveChanges();
        }

        public List<DayOff> GetDaysOffByServiceId(int id)
        {
            return db.Days_off.Where(item => item.Service_id == id).ToList();
        }

        public List<DayTimes> GetDaysTimesByServiceId(int id)
        {
            List<DayTimes> dayTimes = new List<DayTimes> { };
            List<Service_to_time> service_To_Times = db.Service_to_time.Where(item => item.Service_id == id && item.Status.Equals("new")).ToList();
            foreach (Service_to_time service_To_Time in service_To_Times)
            {
                List<DayTimes> dayTimesByDay = dayTimes.Where(item => item.Day.Equals(service_To_Time.Day)).ToList();
                if (dayTimesByDay.Count > 0)
                {
                    dayTimes[dayTimes.IndexOf(dayTimesByDay[0])].Times.Add(service_To_Time.Time);
                } else
                {
                    DayTimes newDayTimes = new DayTimes();
                    newDayTimes.Day = service_To_Time.Day;
                    newDayTimes.Times = new List<string> { service_To_Time.Time };
                    dayTimes.Add(newDayTimes);
                }
            }
            return dayTimes;
        }

        // TODO: доработать
        public ServiceOutput GetServiceById(int id)
        {
            Service service = db.Services.Find(id);
            if (!(service is null))
            {
                ServiceOutput serviceOut = new ServiceOutput {
                    Id = service.Id,
                    Company_id = service.Company_id,
                    Name = service.Name,
                    Description = service.Description,
                    Long_time = service.Long_time,
                    Price = service.Price,
                    Address = db.Company_to_address.Find(service.Address_id).Address,
                    Days = db.Service_to_time.Where(item => item.Service_id == service.Id && item.Status.Equals("new")).Select(item => item.Day).Distinct().ToArray(),
                    Times = db.Service_to_time.Where(item => item.Service_id == service.Id && item.Status.Equals("new")).Select(item => item.Time).ToArray(),
                    DaysOff = db.Days_off.Where(item => item.Service_id == service.Id).Select(item => item.Date).ToArray(),
                    EndDate = service.EndDate   
                };
                return serviceOut;
            }
            return null;
        }

        public List<ServiceOutput> GetServicesByCompanyId(int company_id)
        {
            List<Service> services =  db.Services.Where(service => service.Company_id == company_id).ToList();
            List<ServiceOutput> servicesOut = new List<ServiceOutput> { };
            foreach (Service service in services)
            {
                servicesOut.Add(GetServiceById(service.Id));
            }
            return servicesOut;
        }

        /*
        public List<Service> GetServicesByCompanyAddress(int company_id, string address)
        {
            return db.Services.Where(service => (service.Company_id == company_id) && (service.Address.Contains(address))).ToList();
        }
        */

        // Records queries
        public int AddRecord(int user_id, int service_id, string address, string time, string date)
        {
            int timeId = db.Service_to_time.Where(item => item.Service_id == service_id && item.Time.Equals(time) && item.Status.Equals("new")).ToList()[0].Id;
            Record record = new Record { User_id = user_id, Service_id = service_id, Time_id = timeId, Status = "new", Date = date };
            db.Records.Add(record);
            db.SaveChanges();
            return db.Records.Where(r => r.User_id == user_id && r.Service_id == service_id && r.Time_id == timeId && r.Date.Equals(date)).ToList()[0].Id;
        }

        public List<RecordOutput> GetRecords()
        {
            List<Record> records = db.Records.Where(r => r.Status.Equals("new")).ToList();
            List<RecordOutput> recordOut = new List<RecordOutput> { };
            foreach (Record record in records)
            {
                recordOut.Add(new RecordOutput
                {
                    Id = record.Id,
                    User_id = record.User_id,
                    Service_id = record.Service_id,
                    Date = record.Date,
                    Status = record.Status,
                    Address = db.Company_to_address.Find(db.Services.Find(record.Service_id).Address_id).Address,
                    Time = db.Service_to_time.Find(record.Time_id).Time
                });
            }
            return recordOut;
        }

        // NOT WITH OUTPUT
        public Record GetRecordById(int id)
        {
            return db.Records.Find(id);
        }

        // NOT WITH OUTPUT
        public void UpdateRecord(Record record)
        {
            db.Update(record);
            db.SaveChanges();
        }

        public List<Record> GetRecordsByUserId(int id)
        {
            return db.Records.Where(record => record.User_id == id).ToList();
        }

        public List<Record> GetRecordsByServiceId(int id)
        {
            return db.Records.Where(record => record.Service_id == id).ToList();
        }

        public List<string> GetTimesByDateAndServiceId(string date, int id)
        {
            string dateCompare = DateTime.Parse(date).ToString("yyyy-MM-dd");
            string dayOfWeek = DateTime.Parse(date).DayOfWeek.ToString();
            List<Record> records = db.Records.Where(r => r.Date.Equals(dateCompare) && r.Service_id == id && r.Status.Equals("new")).ToList();
            List<string> allTimes = db.Service_to_time.Where(item => item.Service_id == id && item.Day.Equals(dayOfWeek) && item.Status.Equals("new")).Select(item => item.Time).ToList();
            List<string> times = new List<string> { };
            foreach (Record record in records)
            {
                allTimes.Remove(db.Service_to_time.Find(record.Time_id).Time);
            }
            foreach (string time in allTimes)
            {
                if (DateTime.ParseExact(date.Replace(" 0:00:00", "") + ' ' + time, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture).CompareTo(DateTime.Now) > 0)
                {
                    times.Add(time);
                }
            }
            return times;
        }


        // other queries
        public List<RecordOutputAdmin> GetRecordsByCompanyId(int id)
        {
            return db.Records.Join(db.Services,
                   r => r.Service_id,
                   s => s.Id,
                   (r, s) => new
                   {
                       User_id = r.User_id,
                       Company_id = s.Company_id,
                       Name = s.Name,
                       Address_id = s.Address_id,
                       Time_id = r.Time_id,
                       Date = r.Date,
                       Status = r.Status
                   })
                   .Where(temp => temp.Company_id == id && temp.Status.Equals("new")) 
                   .Join(db.Service_to_time,
                   temp => temp.Time_id,
                   st => st.Id,
                   (temp, st) => new {
                       User_id = temp.User_id,
                       Company_id = temp.Company_id,
                       Name = temp.Name,
                       Address_id = temp.Address_id,
                       Time = st.Time,
                       Date = temp.Date
                   })
                   .Join(db.Company_to_address,
                   temp => temp.Address_id,
                   ca => ca.Id,
                   (temp, ca) => new {
                       User_id = temp.User_id,
                       Company_id = temp.Company_id,
                       Name = temp.Name,
                       Address = ca.Address,
                       Time = temp.Time,
                       Date = temp.Date
                   })
                   .Join(db.Users,
                   temp => temp.User_id,
                   u => u.Id,
                   (temp, u) => new RecordOutputAdmin(u.Name, u.Email, u.Phone, temp.Time, temp.Address, temp.Name, temp.Date))
                   .Distinct()
                   .ToList();
        }

        public List<RecordOutputAdmin> GetRecordsByCompanyIdAndDate(int id, DateTime date)
        {
            string dateString = date.ToString("yyyy-MM-dd");
            return db.Records.Join(db.Services,
                   r => r.Service_id,
                   s => s.Id,
                   (r, s) => new
                   {
                       User_id = r.User_id,
                       Company_id = s.Company_id,
                       Name = s.Name,
                       Address_id = s.Address_id,
                       Time_id = r.Time_id,
                       Date = r.Date,
                       Status = r.Status
                   })
                   .Where(temp => temp.Company_id == id && temp.Date.Equals(dateString) && temp.Status.Equals("new"))
                   .Join(db.Service_to_time,
                   temp => temp.Time_id,
                   st => st.Id,
                   (temp, st) => new {
                       User_id = temp.User_id,
                       Company_id = temp.Company_id,
                       Name = temp.Name,
                       Address_id = temp.Address_id,
                       Time = st.Time,
                       Date = temp.Date
                   })
                   .Join(db.Company_to_address,
                   temp => temp.Address_id,
                   ca => ca.Id,
                   (temp, ca) => new {
                       User_id = temp.User_id,
                       Company_id = temp.Company_id,
                       Name = temp.Name,
                       Address = ca.Address,
                       Time = temp.Time,
                       Date = temp.Date
                   })
                   .Join(db.Users,
                   temp => temp.User_id,
                   u => u.Id,
                   (temp, u) => new RecordOutputAdmin(u.Name, u.Email, u.Phone, temp.Time, temp.Address, temp.Name, temp.Date))
                   .Distinct()
                   .ToList();
        }

        public List<RecordOutputAdmin> GetRecordsBySearchAndCompanyId(int id, string searchStr) 
        {
            string search = searchStr.ToLower();
            List<RecordOutputAdmin> records = db.Records.Join(db.Services,
                   r => r.Service_id,
                   s => s.Id,
                   (r, s) => new
                   {
                       User_id = r.User_id,
                       Company_id = s.Company_id,
                       Name = s.Name,
                       Address_id = s.Address_id,
                       Time_id = r.Time_id,
                       Date = r.Date,
                       Status = r.Status
                   })
                   .Where(temp => temp.Company_id == id && temp.Status.Equals("new"))
                   .Join(db.Service_to_time,
                   temp => temp.Time_id,
                   st => st.Id,
                   (temp, st) => new {
                       User_id = temp.User_id,
                       Company_id = temp.Company_id,
                       Name = temp.Name,
                       Address_id = temp.Address_id,
                       Time = st.Time,
                       Date = temp.Date
                   })
                   .Join(db.Company_to_address,
                   temp => temp.Address_id,
                   ca => ca.Id,
                   (temp, ca) => new {
                       User_id = temp.User_id,
                       Company_id = temp.Company_id,
                       Name = temp.Name,
                       Address = ca.Address,
                       Time = temp.Time,
                       Date = temp.Date
                   })
                   .Join(db.Users,
                   temp => temp.User_id,
                   u => u.Id,
                   (temp, u) => new RecordOutputAdmin(u.Name, u.Email, u.Phone, temp.Time, temp.Address, temp.Name, temp.Date))
                   .Distinct()
                   .ToList();

            return records.Where(r => r.Name.ToLower().Contains(search) || 
                                 r.Email.ToLower().Contains(search) || 
                                 r.Phone.ToLower().Contains(search) || 
                                 r.Time.ToLower().Contains(search) || 
                                 r.Address.ToLower().Contains(search) || 
                                 r.RecordName.ToLower().Contains(search) || 
                                 r.Date.ToLower().Contains(search)).ToList();
        }

        public List<RecordOutputAdmin> GetUsersTimesByServiceId(int id)
        {
            return db.Records.Join(db.Services,
                   r => r.Service_id,
                   s => s.Id,
                   (r, s) => new
                   {
                       User_id = r.User_id,
                       Service_id = r.Service_id,
                       Address_id = s.Address_id,
                       Time_id = r.Time_id,
                       Name = s.Name,
                       Date = r.Date,
                       Status = r.Status
                   })
                   .Where(temp => temp.Service_id == id && temp.Status.Equals("new"))
                   .Join(db.Service_to_time,
                   temp => temp.Time_id,
                   st => st.Id,
                   (temp, st) => new {
                       User_id = temp.User_id,
                       Name = temp.Name,
                       Address_id = temp.Address_id,
                       Time = st.Time,
                       Date = temp.Date
                   })
                   .Join(db.Company_to_address,
                   temp => temp.Address_id,
                   ca => ca.Id,
                   (temp, ca) => new {
                       User_id = temp.User_id,
                       Name = temp.Name,
                       Address = ca.Address,
                       Time = temp.Time,
                       Date = temp.Date
                   })
                   .Join(db.Users,
                   temp => temp.User_id,
                   u => u.Id,
                   (temp, u) => new RecordOutputAdmin(u.Name, u.Email, u.Phone, temp.Time, temp.Address, temp.Name, temp.Date))
                   .Distinct()
                   .ToList();
        }

        public RecordOutputClient GetRecordOutputByRecordId(int id)
        {
            try
            {
                return db.Records.Join(db.Services,
                      r => r.Service_id,
                      s => s.Id,
                      (r, s) => new
                      {
                          RecordId = r.Id,
                          Address_id = s.Address_id,
                          Company_id = s.Company_id,
                          ServiceName = s.Name,
                          Time_id = r.Time_id,
                          Name = s.Name,
                          Date = r.Date

                      })
                      .Where(temp => temp.RecordId == id)
                      .Join(db.Company_to_address,
                      temp => temp.Address_id,
                      ca => ca.Id,
                      (temp, ca) => new
                      {
                          RecordId = temp.RecordId,
                          Company_id = temp.Company_id,
                          Address = ca.Address,
                          Name = temp.Name,
                          ServiceName = temp.ServiceName,
                          Date = temp.Date,
                          Time_id = temp.Time_id
                      })
                      .Join(db.Service_to_time,
                      temp => temp.Time_id,
                      st => st.Id,
                      (temp, st) => new
                      {
                          RecordId = temp.RecordId,
                          Company_id = temp.Company_id,
                          Address = temp.Address,
                          Name = temp.Name,
                          ServiceName = temp.ServiceName,
                          Date = temp.Date,
                          Time = st.Time
                      })
                      .Join(db.Companies,
                      temp => temp.Company_id,
                      c => c.Id,
                      (temp, c) => new RecordOutputClient(temp.RecordId, c.Name, temp.ServiceName, temp.Date, temp.Time, temp.Address))
                      .Distinct()
                      .ToList()[0];
            } catch (Exception)
            {
                return null;
            }
        }
    }
}
