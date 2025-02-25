using Edunext.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using X.PagedList;
using X.PagedList.Extensions;

namespace Edunext.Controllers
{
    public class UserController : Controller
    {
        EdunextContext context = new EdunextContext();
        public IActionResult Login()
        {
            User user = new User();
            return View(user);
        }
        [HttpPost]
        public IActionResult Login(User user)
        {
            User user1 = context.Users.Where(u => u.Email == user.Email).FirstOrDefault();
            if (user1 != null)
            {
                if (user1.Password == user.Password)
                {
                    HttpContext.Session.SetInt32("UserId", user1.Id);
                    HttpContext.Session.SetInt32("Role", user1.Role);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Message = "Wrong Password";
                    return View(user);
                }
            }
            else
            {
                ViewBag.Message = "User does not exist";
                return View(user);
            }
        }
        
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult Register()
        {
            User user = new User();
            user.Role = 1;
            return View(user);
        }
        [HttpPost]
        public IActionResult Register(User user)
        {
            //check valid input
            if (!ModelState.IsValid)
            {
                this.ViewBag.Message = "Invalid input";
                return View(user);
            }
            //check email format
            if (!Regex.IsMatch(user.Email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"))
            {
                this.ViewBag.Message = "Invalid email format";
                return View(user);
            }
            //check password length
            if (user.Password.Length < 8)
            {
                this.ViewBag.Message = "Password must be at least 8 characters";
                return View(user);
            }
            //check role input
            if (user.Role==null||( user.Role != 1 && user.Role != 2))
            {
                this.ViewBag.Message = "Invalid role";
                return View(user);
            }

            //check email already exists
            User user1 = context.Users.Where(u => u.Email == user.Email).FirstOrDefault();
            if (user1 == null)
            {
                context.Users.Add(user);
                context.SaveChanges();
                TempData["Message"] = "Registration successful! Please log in.";
                return RedirectToAction("Login");
            }
            else
            {
                ViewBag.Message = "User already exists";
                return View(user);
            }
        }
        
        public IActionResult Index(int? page)
        {
            int pageSize = 5; // Số item trên mỗi trang
            int pageNumber = (page ?? 1);
            //check if form is submitted
            string search = Request.Query["searchString"];
            if (!string.IsNullOrEmpty(search))
            {
                var users = context.Users.Where(u => u.Email.Contains(search)||u.Code.Contains(search)).OrderBy(u => u.Id).ToPagedList(pageNumber, pageSize);
                return View(users);
            }
            else
            {
                var users = context.Users.OrderBy(u => u.Id).ToPagedList(pageNumber, pageSize);
                return View(users);
            }
            
        }
    }
}
