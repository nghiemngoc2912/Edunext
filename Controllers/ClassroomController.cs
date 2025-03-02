using Edunext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Edunext.Controllers
{
    public class ClassroomController : Controller
    {
        EdunextContext context = new EdunextContext();
        public IActionResult Index()
        {
            var classes=context.Classrooms.Include(c=>c.Semester).Include(c=>c.Teacher).Include(c=>c.Course).Where(c=>c.IsDeleted==false).ToList();
            return View(classes);
        }
    }
}
