using Edunext.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using X.PagedList;
using X.PagedList.Extensions;
using Edunext.Helpers;
using Microsoft.AspNetCore.Identity;
using System.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Edunext.Filters;

namespace Edunext.Controllers
{
    public class UserController : Controller
    {
        EdunextContext context = new EdunextContext();
        private readonly EmailHelper _emailHelper;
        public UserController(EmailHelper emailHelper)
        {
            _emailHelper = emailHelper;
        }
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
        [LoginFilter]
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

        [RoleFilter(4)]
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
        [RoleFilter(4)]
        public IActionResult Create()
        {
            User user = new User();
            return View(user);
        }
        [RoleFilter(4)]
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
        [RoleFilter(4)]
        public IActionResult Edit(int id)
        {
            User user = context.Users.Find(id);
            return View(user);
        }
        [RoleFilter(4)]
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
                this.ViewBag.Message = "Title cannot be null or space";
                return View(user);
            }
            //check code exists
            User user2 = context.Users.Where(u => u.Code == user.Code).FirstOrDefault();
            if (user2 != null&&user2.Id!=user.Id)
            {
                this.ViewBag.Message = "Title already exists";
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
        [RoleFilter(4)]
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
        public IActionResult Details(int id)
        {
            User user = context.Users.Find(id);
            return View(user);
        }
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ChangePassword(IFormCollection form)
        {
            string oldPassword = Request.Form["OldPassword"];
            string newPassword = Request.Form["NewPassword"];
            //check input: password length
            //check password length
            if (!ValidationHelper.IsValidPassword(newPassword))
            {
                this.ViewBag.Message = "Password must be at least 8 characters";
                return View();
            }

            User user1 = context.Users.Find(HttpContext.Session.GetInt32("UserId"));
            if(user1.Password!=oldPassword)
            {
                this.ViewBag.Message = "Old password is incorrect";
                return View();
            }
            user1.Password = newPassword;
            user1.UpdatedAt = System.DateTime.Now;
            user1.UpdatedBy = HttpContext.Session.GetInt32("UserId");
            context.Update(user1);
            context.SaveChanges();
            TempData["Message"] = "Change password successful!";
            return RedirectToAction("Details", new { id = user1.Id });
        }
        public IActionResult EditNorm()
        {
            var user = context.Users.Find(HttpContext.Session.GetInt32("UserId"));
            return View(user);
        }
        [HttpPost]
        public IActionResult EditNorm(User user)
        {
            if (!ValidationHelper.IsValidEmail(user.Email))
            {
                this.ViewBag.Message = "Invalid email format";
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
            if (user2 != null && user2.Id != user.Id)
            {
                this.ViewBag.Message = "Code already exists";
                return View(user);
            }

            //check email already exists
            User user1 = context.Users.Where(u => u.Email == user.Email).FirstOrDefault();
            if (user1 == null || user1.Id == user.Id)
            {
                //updated_at
                //by=adminId as default
                User updateUser = context.Users.Find(user.Id);
                updateUser.FirstName = user.FirstName;
                updateUser.LastName = user.LastName;
                updateUser.Email = user.Email;
                updateUser.Code = user.Code;
                updateUser.UpdatedAt = System.DateTime.Now;
                updateUser.UpdatedBy = HttpContext.Session.GetInt32("UserId");
                context.Update(updateUser);
                context.SaveChanges();
                TempData["Message"] = "Edit successful!";
                return RedirectToAction("Details", new { id = user1.Id });
            }
            else
            {
                ViewBag.Message = "User already exists with same email";
                return View(user);
            }
        }
        public IActionResult PreResetPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> PreResetPassword(string email)
        {
            var user = context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Message = "Email không tồn tại!";
                return View();
            }

            // Mã hóa email bằng AES
            string encryptedEmail = Encrypt(email);

            // Tạo link reset mật khẩu
            string resetLink = Url.Action("ResetPassword", "User", new { token = encryptedEmail }, Request.Scheme);

            // Gửi email cho người dùng
            await _emailHelper.SendEmailAsync(email, "Reset Password", $"Click vào link để đặt lại mật khẩu: <a href='{resetLink}'>Reset Password</a>");

            ViewBag.Message = "Vui lòng kiểm tra email để đặt lại mật khẩu!";
            return View();
        }
        private string Encrypt(string text)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes("nghiemngoc291204nghiemngoc291204"); // 32 ký tự
                aes.IV = Encoding.UTF8.GetBytes("nghiemngoc291204"); // 16 ký tự

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(text);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
        }
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            try
            {
                string email = Decrypt(token);
                if (string.IsNullOrEmpty(email))
                    return BadRequest("Token không hợp lệ!");

                ViewBag.Email = email;
                ViewBag.Token = token; // Giữ token để gửi lại khi đặt mật khẩu mới
                return View();
            }
            catch
            {
                return BadRequest("Token không hợp lệ!");
            }
        }

        // Hàm giải mã email
        private string Decrypt(string encryptedText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes("nghiemngoc291204nghiemngoc291204"); // Phải giống với key mã hóa
                aes.IV = Encoding.UTF8.GetBytes("nghiemngoc291204"); // Phải giống với IV mã hóa

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }

        [HttpPost]
        public IActionResult ResetPassword(IFormCollection form)
        {
            string email = Request.Form["email"];
            string newPassword = Request.Form["password"];
            //check input: password length
            //check password length
            if (!ValidationHelper.IsValidPassword(newPassword))
            {
                this.ViewBag.Message = "Password must be at least 8 characters";
                return View();
            }
            var user = context.Users.FirstOrDefault(u => u.Email == email);
            user.Password = newPassword;
            user.UpdatedAt = System.DateTime.Now;
            user.UpdatedBy = user.Id;
            context.Update(user);
            context.SaveChanges();
            TempData["Message"] = "Reset password successful!";
            return RedirectToAction("Login");
        }
    }
}

