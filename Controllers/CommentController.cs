using Edunext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Edunext.Controllers
{
    public class CommentController : Controller
    {
        EdunextContext context = new EdunextContext();
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(int questionId, int userId, string content)
        {
            //check valid
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Message"] = "Content is required";
                return RedirectToAction("Details", "Question", new { id = questionId });
            }
            //check real time exceed to time
            var question = context.Questions.Find(questionId);
            if (question == null)
            {
                TempData["Message"] = "Question not found";
                return RedirectToAction("Details", "Question", new { id = questionId });
            }
            if (question.ToTime < DateTime.Now)
            {
                TempData["Message"] = "Question is expired";
                return RedirectToAction("Details", "Question", new { id = questionId });
            }

            var comment = new Comment
            {
                QuestionId = questionId,
                UserId = userId,
                Content = content
            };
            context.Comments.Add(comment);
            context.SaveChanges();
            TempData["Message"] = "Comment added successfully";
            return RedirectToAction("Details", "Question", new { id = questionId });
        }
        [HttpPost]
        public IActionResult Edit([FromBody] Comment model)
        {
            var comment = context.Comments.FirstOrDefault(c => c.Id == model.Id);
            if (comment == null)
            {
                return Json(new { success = false });
            }
            //check valid
            if (string.IsNullOrWhiteSpace(model.Content))
            {
                return Json(new { success = false });
            }
            var question = context.Questions.Find(comment.QuestionId);
            if (question == null)
            {
                return Json(new { success = false });
            }
            if (question.ToTime < DateTime.Now)
            {
                return Json(new { success = false });
            }

            comment.Content = model.Content;
            comment.UpdatedAt = DateTime.Now;
            context.Comments.Update(comment);
            context.SaveChanges();

            return Json(new { success = true });
        }

        public IActionResult Delete(int id)
        {
            var comment = context.Comments.Find(id);
            context.Comments.Remove(comment);
            context.SaveChanges();
            TempData["Message"] = "Comment deleted successfully";
            return RedirectToAction("Details","Question", new {id=comment.QuestionId});
        }
    }
}
