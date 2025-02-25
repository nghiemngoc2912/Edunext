using Edunext.Models;
using Microsoft.AspNetCore.Mvc;

namespace Edunext.Controllers
{
    public class CourseController : Controller
    {
        EdunextContext context = new EdunextContext();
        public IActionResult Index(int userId)
        {
            //find classes that the user is enrolled in
            var classes = context.ClassEnrollments.Where(e => e.UserId == userId).Select(e => e.Class).ToList();
            //find courses that the classes are enrolled in
            var courses = context.Classrooms.Where(c => classes.Contains(c)).Select(c => c.Course).ToList();
            return View(courses);
        }
    }
}
