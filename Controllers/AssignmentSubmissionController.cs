using Edunext.Filters;
using Edunext.Models;
using Microsoft.AspNetCore.Mvc;

namespace Edunext.Controllers
{
    [RoleFilter(1,2)]
    public class AssignmentSubmissionController : Controller
    {
        EdunextContext context = new EdunextContext();
        [RoleFilter(1)]
        [HttpPost]
        public IActionResult Create(int assignmentId, int userId, string fileLink)
        {
            //check valid
            if (string.IsNullOrWhiteSpace(fileLink))
            {
                TempData["Message"] = "File link is required";
                return RedirectToAction("Details", "Assignment", new { id = assignmentId });
            }
            //check real time exceed to time
            var assignment = context.Assignments.Find(assignmentId);
            if (assignment == null)
            {
                TempData["Message"] = "Assignment not found";
                return RedirectToAction("Details", "Assignment", new { id = assignmentId });
            }
            if (assignment.DueDate < DateOnly.FromDateTime(DateTime.Now))
            {
                TempData["Message"] = "Assignment is expired";
                return RedirectToAction("Details", "Assignment", new { id = assignmentId });
            }
            AssignmentSubmission assignmentSubmission = new AssignmentSubmission
            {
                AssignmentId = assignmentId,
                UserId = userId,
                FileLink = fileLink,
                SubmissionDate = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            context.AssignmentSubmissions.Add(assignmentSubmission);
            context.SaveChanges();
            TempData["Message"] = "Assignment submitted successfully";
            return RedirectToAction("Details", "Assignment", new { id = assignmentId });
        }
        [RoleFilter(1)]
        [HttpPost]
        public IActionResult Edit(int assignmentId, int userId, string fileLink)
        {
            //check valid
            if (string.IsNullOrWhiteSpace(fileLink))
            {
                TempData["Message"] = "File link is required";
                return RedirectToAction("Details", "Assignment", new { id = assignmentId });
            }
            //check real time exceed to time
            var assignment = context.Assignments.Find(assignmentId);
            if (assignment == null)
            {
                TempData["Message"] = "Assignment not found";
                return RedirectToAction("Details", "Assignment", new { id = assignmentId });
            }
            if (assignment.DueDate < DateOnly.FromDateTime(DateTime.Now))
            {
                TempData["Message"] = "Assignment is expired";
                return RedirectToAction("Details", "Assignment", new { id = assignmentId });
            }
            var assignmentSubmission = context.AssignmentSubmissions
                .FirstOrDefault(s => s.AssignmentId == assignmentId && s.UserId == userId);
            if (assignmentSubmission == null)
            {
                TempData["Message"] = "Assignment submission not found";
                return RedirectToAction("Details", "Assignment", new { id = assignmentId });
            }
            assignmentSubmission.FileLink = fileLink;
            assignmentSubmission.SubmissionDate = DateTime.Now;
            assignmentSubmission.UpdatedAt = DateTime.Now;
            context.SaveChanges();
            TempData["Message"] = "Assignment submitted successfully";
            return RedirectToAction("Details", "Assignment", new { id = assignmentId });
        }
        [RoleFilter(1)]
        public IActionResult Delete(int submissionId, int assignmentId)
        {
            var assignmentSubmission = context.AssignmentSubmissions
                .FirstOrDefault(s => s.Id==submissionId);
            if (assignmentSubmission == null)
            {
                TempData["Message"] = "Assignment submission not found";
                return RedirectToAction("Details", "Assignment", new { id = assignmentId });
            }
            context.AssignmentSubmissions.Remove(assignmentSubmission);
            context.SaveChanges();
            TempData["Message"] = "Assignment submission deleted successfully";
            return RedirectToAction("Details", "Assignment", new { id = assignmentId });
        }
        [RoleFilter(2)]
        public IActionResult Grade(int submissionId, decimal grade, int assignmentId)
        {
            var assignmentSubmission = context.AssignmentSubmissions
                .FirstOrDefault(s => s.Id == submissionId);
            if (assignmentSubmission == null)
            {
                TempData["Message"] = "Assignment submission not found";
                return RedirectToAction("Details", "Assignment", new { id = assignmentId});
            }
            assignmentSubmission.Grade = grade;
            assignmentSubmission.UpdatedAt = DateTime.Now;
            context.SaveChanges();
            TempData["Message"] = "Assignment graded successfully";
            return RedirectToAction("Details", "Assignment", new { id = assignmentId });
        }
    }
}
