using Microsoft.AspNetCore.Mvc;

namespace Edunext.Controllers
{
    public class SlotController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
