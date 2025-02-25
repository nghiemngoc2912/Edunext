using Microsoft.AspNetCore.Mvc;

namespace Edunext.Controllers
{
    public class CommentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
