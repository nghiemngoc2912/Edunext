using Edunext.Models;
using Microsoft.AspNetCore.Mvc;

namespace Edunext.Controllers
{
    public class ClassEnrollmentController : Controller
    {
        EdunextContext context = new EdunextContext();
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(IFormCollection form)
        {
            int userId = (int)HttpContext.Session.GetInt32("UserId");
            string className = form["ClassName"];
            Classroom classroom = context.Classrooms.Where(c => c.Name == className).FirstOrDefault();
            if (classroom == null)
            {
                TempData["Message"] = "Class does not exist";
                return RedirectToAction("Index","Classroom", new { userId = userId });
            }
            else
            {
                //check if user is already enrolled in a class with the same course
                var classes = context.ClassEnrollments.Where(e => e.UserId == userId).Select(e => e.Class).ToList();
                var courses = context.Classrooms.Where(c => classes.Contains(c)).Select(c => c.Course).ToList();
                if (courses.Contains(classroom.Course))
                {
                    TempData["Message"] = "You are already enrolled in a class with the same course";
                    return RedirectToAction("Index", "Classroom", new { userId = userId });
                }
                ClassEnrollment enrollment = new ClassEnrollment();
                enrollment.ClassId = classroom.Id;
                enrollment.UserId = userId;
                context.ClassEnrollments.Add(enrollment);
                context.SaveChanges();
                return RedirectToAction("Index", "Classroom", new { userId = userId });
            }
        }
    }
}
