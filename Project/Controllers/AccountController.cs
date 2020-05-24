using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Project.Models;
using Project.Options;
using Project.Context;
using System.Text;
using Project.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace Project.Controllers
{
    public class AccountController : Controller
    {
        readonly DbService db = new DbService();
        IWebHostEnvironment appEnvironment;

        public AccountController(IWebHostEnvironment appEnvironment)
        {
            this.appEnvironment = appEnvironment;
        }


        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var identity = PasswordService.GetIdentity(email, password, db);
            if (identity == null)
            {
                return BadRequest(new { errorText = "Неправильное имя пользователя или пароль." });
            }

            var now = DateTime.UtcNow;
            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            HttpContext.Session.SetString("JWToken", encodedJwt);
            var response = new
            {
                access_token = encodedJwt
            };

            return Json(response);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet]
        public IActionResult Logoff()
        {
            HttpContext.Session.Clear();
            return Redirect("~/Home/Index");
        }

        [HttpGet]
        public IActionResult AddCompany()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("/register")]
        public async Task<IActionResult> Register(string name, string phone, string email, string address, string description, IFormFile pic, string password)
        {
            if (db.GetCompanyByEmail(email) is null)
            {
                string filePath = String.Empty;
                if (!(pic is null)) {
                    filePath = "/Files/" + Guid.NewGuid() + "." + pic.FileName.Split('.')[1];
                    using (var fileStream = new FileStream(appEnvironment.WebRootPath + filePath, FileMode.Create))
                    {
                        await pic.CopyToAsync(fileStream);
                    }
                }
                Console.WriteLine(address);
                string[] addressArray = new string[] { };
                if (!(address is null))
                {
                    addressArray = address.Substring(0, address.Length - 1).Split(';');
                }
                db.AddCompany(name, phone, email, addressArray, description, filePath, 
                    Convert.ToBase64String(PasswordService.HashPassword(password, RandomNumberGenerator.Create())), "company");
                return Json(new { Result = "success" });
            }
            return Json(new { Result = "failed" });
        }
    }   
}