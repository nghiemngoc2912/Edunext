using Microsoft.AspNetCore.Mvc;

namespace Edunext.Controllers
{
    public class QuestionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
