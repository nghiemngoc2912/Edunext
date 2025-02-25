using Edunext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Edunext.Controllers
{
    public class ClassroomController : Controller
    {
        EdunextContext context = new EdunextContext();
        public IActionResult Index(int userId)
        {
            var classes = context.ClassEnrollments
                .Where(e => e.UserId == userId)
                .Include(e => e.Class) // Nạp thông tin lớp học
                    .ThenInclude(c => c.Course)
                .Select(e => new {
                    Class=e.Class,
                    Semester=e.Class.Semester
                }
                    )
                .ToList();
            return View(classes);
        }
    }
}
