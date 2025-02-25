using Edunext.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using X.PagedList;
using X.PagedList.Extensions;
using Edunext.Helpers;

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
            //login need to be active
            //save session for user id and role
            User user1 = context.Users.Where(u => u.Email == user.Email).FirstOrDefault();
            if (user1 != null)
            {
                if (user1.IsDeleted == true)
                {
                    ViewBag.Message = "User is inactive";
                    return View(user);
                }
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
            //clear session
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
            //check input: email format, password length, role, email already exists, firstname, lastname not null or space

            //check email format
            if (!ValidationHelper.IsValidEmail(user.Email))
            {
                this.ViewBag.Message = "Invalid email format";
                return View(user);
            }
            //check password length
            if (!ValidationHelper.IsValidPassword(user.Password))
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
            if (!ValidationHelper.CheckInputName(user.FirstName))
            {
                this.ViewBag.Message = "First name cannot be null or space and just includes letter";
                return View(user);
            }
            if (!ValidationHelper.CheckInputName(user.LastName))
            {
                this.ViewBag.Message = "Last name cannot be null or space and just includes letter";
                return View(user);
            }
            if(!ValidationHelper.CheckInputNotSpace(user.Code))
            {
                this.ViewBag.Message = "Code cannot be null or space";
                return View(user);
            }
            //check code exists
            User user2 = context.Users.Where(u => u.Code == user.Code).FirstOrDefault();
            if (user2 != null)
            {
                this.ViewBag.Message = "Code already exists";
                return View(user);
            }
            //check email already exists
            User user1 = context.Users.Where(u => u.Email == user.Email).FirstOrDefault();
            if (user1 == null)
            {
                //updated_at
                //by=null as default
                user.UpdatedAt = System.DateTime.Now;
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
            //return list of users who are active
            //pagination
            int pageSize = 5; // Số item trên mỗi trang
            int pageNumber = (page ?? 1);
            //check if form is submitted
            string search = Request.Query["searchString"];
            if (!string.IsNullOrEmpty(search))
            {
                var users = context.Users.Where(u => u.IsDeleted==false&&(u.Email.Contains(search)||u.Code.Contains(search))).OrderBy(u => u.Id).ToPagedList(pageNumber, pageSize);
                ViewData["CurrentFilter"]= search;
                return View(users);
            }
            else
            {
                var users = context.Users.Where(u=>u.IsDeleted==false).OrderBy(u => u.Id).ToPagedList(pageNumber, pageSize);
                return View(users);
            }
            
        }
        public IActionResult Create()
        {
            User user = new User();
            return View(user);
        }
        [HttpPost]
        public IActionResult Create(User user)
        {
            //check input: email format, password length, role, email already exists,firstname, lastname not null or space

            //check email format
            if (!ValidationHelper.IsValidEmail(user.Email))
            {
                this.ViewBag.Message = "Invalid email format";
                return View(user);
            }
            //check password length
            if (!ValidationHelper.IsValidPassword(user.Password))
            {
                this.ViewBag.Message = "Password must be at least 8 characters";
                return View(user);
            }
            //check role input
            if (user.Role == null || (user.Role != 1 && user.Role != 2&&user.Role!=3))
            {
                this.ViewBag.Message = "Invalid role";
                return View(user);
            }
            //check firstname, lastname not null or space
            if(!ValidationHelper.CheckInputName(user.FirstName))
            {
                this.ViewBag.Message = "First name cannot be null or space and just includes letter";
                return View(user);
            }
            if (!ValidationHelper.CheckInputName(user.LastName))
            {
                this.ViewBag.Message = "Last name cannot be null or space and just includes letter";
                return View(user);
            }
            if (!ValidationHelper.CheckInputNotSpace(user.Code))
            {
                this.ViewBag.Message = "Code cannot be null or space";
                return View(user);
            }
            //check code exists
            User user2 = context.Users.Where(u => u.Code == user.Code).FirstOrDefault();
            if (user2 != null)
            {
                this.ViewBag.Message = "Code already exists";
                return View(user);
            }

            //check email already exists
            User user1 = context.Users.Where(u => u.Email == user.Email).FirstOrDefault();
            if (user1 == null)
            {
                //updated_at
                //by=adminId as default
                user.UpdatedBy=HttpContext.Session.GetInt32("UserId");
                user.UpdatedAt = System.DateTime.Now;
                context.Users.Add(user);
                context.SaveChanges();
                TempData["Message"] = "Create successful!";
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Message = "User already exists";
                return View(user);
            }
        }
        public IActionResult Edit(int id)
        {
            User user = context.Users.Find(id);
            return View(user);
        }
        [HttpPost]
        public IActionResult Edit(User user)
        {
            //check input: email format, password length, role, email already exists,firstname, lastname not null or space

            //check email format
            if (!ValidationHelper.IsValidEmail(user.Email))
            {
                this.ViewBag.Message = "Invalid email format";
                return View(user);
            }
            //check role input
            if (user.Role == null || (user.Role != 1 && user.Role != 2 && user.Role != 3))
            {
                this.ViewBag.Message = "Invalid role";
                return View(user);
            }
            //check firstname, lastname not null or space
            if (!ValidationHelper.CheckInputName(user.FirstName))
            {
                this.ViewBag.Message = "First name cannot be null or space and just includes letter";
                return View(user);
            }
            if (!ValidationHelper.CheckInputName(user.LastName))
            {
                this.ViewBag.Message = "Last name cannot be null or space and just includes letter";
                return View(user);
            }
            if (!ValidationHelper.CheckInputNotSpace(user.Code))
            {
                this.ViewBag.Message = "Code cannot be null or space";
                return View(user);
            }
            //check code exists
            User user2 = context.Users.Where(u => u.Code == user.Code).FirstOrDefault();
            if (user2 != null&&user2.Id!=user.Id)
            {
                this.ViewBag.Message = "Code already exists";
                return View(user);
            }

            //check email already exists
            User user1 = context.Users.Where(u => u.Email == user.Email).FirstOrDefault();
            if (user1 == null||user1.Id==user.Id)
            {
                //updated_at
                //by=adminId as default
                User updateUser=context.Users.Find(user.Id);
                updateUser.FirstName = user.FirstName;
                updateUser.LastName = user.LastName;
                updateUser.Email = user.Email;
                updateUser.Role = user.Role;
                updateUser.Code = user.Code;
                updateUser.UpdatedAt = System.DateTime.Now;
                updateUser.UpdatedBy = HttpContext.Session.GetInt32("UserId");
                context.Update(updateUser);
                context.SaveChanges();
                TempData["Message"] = "Edit successful!";
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Message = "User already exists with same email";
                return View(user);
            }
        }
        public IActionResult Delete(int id)
        {
            //khong xoa duoc admin

            User user = context.Users.Find(id);
            if (user.Role == 4)
            {
                TempData["Message"] = "Cannot delete admin!";
                return RedirectToAction("Index");
            }
            user.IsDeleted = true;
            user.UpdatedBy = HttpContext.Session.GetInt32("UserId");
            user.UpdatedAt = System.DateTime.Now;
            context.Update(user);
            context.SaveChanges();
            TempData["Message"] = "Delete successful!";
            return RedirectToAction("Index");
        }
    }
}

