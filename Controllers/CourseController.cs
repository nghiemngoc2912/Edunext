using Edunext.Filters;
using Edunext.Helpers;
using Edunext.Models;
using Microsoft.AspNetCore.Mvc;
using X.PagedList.Extensions;

namespace Edunext.Controllers
{
    [RoleFilter(3)]
    public class CourseController : Controller
    {
        EdunextContext context = new EdunextContext();
        public IActionResult Index(int? page)
        {
            //pagination
            int pageSize = 5; // Số item trên mỗi trang
            int pageNumber = (page ?? 1);
            //check if form is submitted
            string search = Request.Query["searchString"];
            if (!string.IsNullOrEmpty(search))
            {
                var courses = context.Courses.Where(u => u.IsDeleted == false && (u.Code.Contains(search) || u.Name.Contains(search))||u.Description.Contains(search)).OrderBy(u => u.Id).ToPagedList(pageNumber, pageSize);
                ViewData["CurrentFilter"] = search;
                return View(courses);
            }
            else
            {
                var courses = context.Courses.Where(u => u.IsDeleted == false).OrderBy(u => u.Id).ToPagedList(pageNumber, pageSize);
                return View(courses);
            }
        }
        public IActionResult Create()
        {
            Course course = new Course();
            return View(course);
        }
        [HttpPost]
        public IActionResult Create(Course course)
        {
            //check code format, code exists
            //check name format
            if(!ValidationHelper.CheckInputNotSpace(course.Code))
            {
                ViewBag.Message = "Invalid code";
                return View(course);
            }
            if(!ValidationHelper.CheckInputNotSpace(course.Name))
            {
                ViewBag.Message = "Invalid name";
                return View(course);
            }
            if(context.Courses.Any(c => c.Code == course.Code))
            {
                ViewBag.Message = "Title exists";
                return View(course);
            }
            course.UpdatedAt = DateTime.Now;
            course.UpdatedBy = HttpContext.Session.GetInt32("UserId");
            context.Courses.Add(course);
            context.SaveChanges();
            TempData["Message"] = "Create course successfully";
            return RedirectToAction("Index");
        }
        public IActionResult Edit(int id)
        {
            Course course = context.Courses.Find(id);
            return View(course);
        }
        [HttpPost]
        public IActionResult Edit(Course course)
        {
            //check code format, code exists
            //check name format
            if (!ValidationHelper.CheckInputNotSpace(course.Code))
            {
                ViewBag.Message = "Invalid code";
                return View(course);
            }
            if (!ValidationHelper.CheckInputNotSpace(course.Name))
            {
                ViewBag.Message = "Invalid name";
                return View(course);
            }
            if (context.Courses.Any(c => c.Code == course.Code && c.Id != course.Id))
            {
                ViewBag.Message = "Code exists";
                return View(course);
            }
            Course updateCourse= context.Courses.Find(course.Id);
            updateCourse.Code = course.Code;
            updateCourse.Name = course.Name;
            updateCourse.Description = course.Description;
            updateCourse.UpdatedAt = DateTime.Now;
            updateCourse.UpdatedBy = HttpContext.Session.GetInt32("UserId");
            context.Courses.Update(updateCourse);
            context.SaveChanges();
            TempData["Message"] = "Update course successfully";
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            Course course = context.Courses.Find(id);
            course.IsDeleted = true;
            course.UpdatedAt = DateTime.Now;
            course.UpdatedBy = HttpContext.Session.GetInt32("UserId");
            context.Courses.Update(course);
            context.SaveChanges();
            TempData["Message"] = "Delete course successfully";
            return RedirectToAction("Index");
        }
    }
}
