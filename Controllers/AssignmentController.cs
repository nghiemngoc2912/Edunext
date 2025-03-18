using Edunext.Filters;
using Edunext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Edunext.Controllers
{
    [LoginFilter]
    [RoleFilter(1, 2)]
    public class AssignmentController : Controller
    {
        EdunextContext context = new EdunextContext();
        public IActionResult Details(int id)
        {
            var assignment = context.Assignments
                .FirstOrDefault(a => a.Id == id);
            //check if the user submitted the assignment
            var userId = HttpContext.Session.GetInt32("UserId"); //get from session
            ViewBag.Submission= context.AssignmentSubmissions
                .FirstOrDefault(s => s.AssignmentId == id && s.UserId == userId);
            ViewBag.SubmissionList = context.AssignmentSubmissions
                .Include(s=>s.User)
                .Where(s => s.AssignmentId == id);
            return View(assignment);
        }
        [RoleFilter(2)]
        public IActionResult Create(int classSlotId)
        {
            var assignment = new Assignment();
            assignment.ClassSlotId = classSlotId;
            return View(assignment);
        }
        [RoleFilter(2)]
        [HttpPost]
        public IActionResult Create(Assignment assignment)
        {
            //check title, description is null or space
            //check due date is in the future
            //check class slot id is valid
            if (string.IsNullOrWhiteSpace(assignment.Title))
            {
                @ViewBag.Message = "Invalid title";
                return View(assignment);
            }
            if (string.IsNullOrWhiteSpace(assignment.Description))
            {
                @ViewBag.Message = "Invalid description";
                return View(assignment);
            }
            if (assignment.DueDate < DateOnly.FromDateTime(DateTime.Now))
            {
                @ViewBag.Message = "Invalid due date";
                return View(assignment);
            }
            if (context.ClassSlotContents.Find(assignment.ClassSlotId) == null)
            {
                @ViewBag.Message = "Invalid class slot id";
                return View(assignment);
            }
            else {
                context.Assignments.Add(assignment);
                context.SaveChanges();
                TempData["Message"] = "Assignment created successfully";
                //redirect to ClassSlotController.Details
                return RedirectToAction("Details", "ClassSlotContent", new { id = assignment.ClassSlotId });
            }
        }
        [RoleFilter(2)]
        public IActionResult Edit(int id)
        {
            var assignment = context.Assignments
                .FirstOrDefault(a => a.Id == id);
            return View(assignment);
        }
        [RoleFilter(2)]
        [HttpPost]
        public IActionResult Edit(Assignment assignment)
        {
            //check title, description is null or space
            //check due date is in the future
            //check class slot id is valid
            if (string.IsNullOrWhiteSpace(assignment.Title))
            {
                @ViewBag.Message = "Invalid title";
                return View(assignment);
            }
            if (string.IsNullOrWhiteSpace(assignment.Description))
            {
                @ViewBag.Message = "Invalid description";
                return View(assignment);
            }
            if (assignment.DueDate < DateOnly.FromDateTime(DateTime.Now))
            {
                @ViewBag.Message = "Invalid due date";
                return View(assignment);
            }
            if (context.ClassSlotContents.Find(assignment.ClassSlotId) == null)
            {
                @ViewBag.Message = "Invalid class slot id";
                return View(assignment);
            }
            else
            {
                var updatedAssignment = context.Assignments.Find(assignment.Id);
                updatedAssignment.Title = assignment.Title;
                updatedAssignment.Description = assignment.Description;
                updatedAssignment.DueDate = assignment.DueDate;
                updatedAssignment.ClassSlotId = assignment.ClassSlotId;
                context.Assignments.Update(updatedAssignment);
                context.SaveChanges();
                TempData["Message"] = "Assignment updated successfully";
                //redirect to ClassSlotController.Details
                return RedirectToAction("Details", "ClassSlotContent", new { id = assignment.ClassSlotId });
            }
        }
        [RoleFilter(2)]
        public IActionResult Delete(int id)
        {
            var assignment = context.Assignments
                .FirstOrDefault(a => a.Id == id);
            assignment.IsDeleted = true;
            context.Assignments.Update(assignment);
            context.SaveChanges();
            //redirect to ClassSlotController.Details
            TempData["Message"] = "Assignment deleted successfully";
            return RedirectToAction("Details", "ClassSlotContent", new { id = assignment.ClassSlotId });
        }
    }
}
