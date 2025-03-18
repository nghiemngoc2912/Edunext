using Edunext.Filters;
using Edunext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Edunext.Controllers
{
    [RoleFilter(1,2)]
    public class ClassSlotContentController : Controller
    {
        EdunextContext context = new EdunextContext();
        public IActionResult Details(int classId,int slotId)
        {
            var classSlotContent = context.ClassSlotContents
                .Include(c=>c.Questions)
                .Include(c=>c.Assignments)
                .FirstOrDefault(c => c.ClassId == classId && c.SlotId == slotId);
            return View(classSlotContent);
        }
        [HttpGet("ClassSlotContent/Details/{id}")]
        public IActionResult Details(int id)
        {
            var classSlotContent = context.ClassSlotContents
                .Include(c=>c.Questions)
                .Include(c=>c.Assignments)
                .FirstOrDefault(c => c.Id == id);
            return View(classSlotContent);
        }
    }
}
