using Microsoft.AspNetCore.Mvc;

namespace Edunext.Controllers
{
    public class SemesterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
