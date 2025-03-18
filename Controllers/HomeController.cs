using Edunext.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Edunext.Controllers
{
    [LoginFilter]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
