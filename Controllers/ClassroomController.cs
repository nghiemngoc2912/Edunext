using Edunext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace Edunext.Controllers
{
    public class ClassroomController : Controller
    {
        EdunextContext context = new EdunextContext();
        public IActionResult Index(int? page)
        {
            int pageSize = 5; // Số item trên mỗi trang
            int pageNumber = (page ?? 1);
            //check if form is submitted
            string search = Request.Query["searchString"];
            if (!string.IsNullOrEmpty(search))
            {
                var classes = context.Classrooms.Include(c => c.Course).Include(c => c.Teacher).Include(c => c.Semester).Where(c => (c.IsDeleted == false)&&(c.Name.Contains(search))).OrderBy(u => u.Id).ToPagedList(pageNumber, pageSize);
                ViewData["CurrentFilter"] = search;
                return View(classes);
            }
            else 
            {
                var classes = context.Classrooms.Include(c => c.Course).Include(c => c.Teacher).Include(c => c.Semester).Where(c => c.IsDeleted == false).OrderBy(u => u.Id).ToPagedList(pageNumber, pageSize);
                return View(classes);
            }
            
        }
        public IActionResult NormIndex(int? page,int? userId) {
            int pageSize = 5; // Số item trên mỗi trang
            int pageNumber = (page ?? 1);
            int role= (int)HttpContext.Session.GetInt32("Role");
            if (role==2)
            {
                var classes = context.Classrooms.Include(c => c.Course).Include(c => c.Teacher).Include(c => c.Semester).Where(c => (c.IsDeleted == false)&&(c.TeacherId==userId)).OrderBy(u => u.Id).ToPagedList(pageNumber, pageSize);
                return View(classes);
            }
            else
            {
                var classes = context.ClassEnrollments
                    .Include(c => c.Class)
                        .ThenInclude(cl => cl.Course) // Include Course của Class
                    .Include(c => c.Class)
                        .ThenInclude(cl => cl.Semester) // Include Semester của Class
                    .Include(c => c.Class)
                        .ThenInclude(cl => cl.Teacher) // Include Teacher của Class
                    .Where(c => c.IsDeleted==false && c.UserId == userId)
                    .Select(c => c.Class)
                    .OrderBy(c => c.Id)
                    .ToPagedList(pageNumber, pageSize);

                return View(classes);
            }
        }

        public IActionResult Create()
        {
            Classroom classroom = new Classroom();
            return View(classroom);
        }



        public IActionResult Delete(int id)
        {
            Classroom classroom = context.Classrooms.Find(id);
            classroom.IsDeleted = true;
            classroom.UpdatedAt = DateTime.Now;
            classroom.UpdatedBy = HttpContext.Session.GetInt32("UserId");
            context.Classrooms.Update(classroom);
            context.SaveChanges();
            TempData["Message"] = "Delete classroom successfully";
            return RedirectToAction("Index");
        }
    }
}
