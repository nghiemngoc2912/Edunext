using Edunext.Helpers;
using Edunext.Models;
using Microsoft.AspNetCore.Mvc;
using X.PagedList.Extensions;

namespace Edunext.Controllers
{
    public class SemesterController : Controller
    {
        EdunextContext context = new EdunextContext();

        public IActionResult Index(int? page)
        {
            int pageSize = 5; // Số item trên mỗi trang
            int pageNumber = (page ?? 1);
            //check if form is submitted
            string search = Request.Query["searchString"];
            if (!string.IsNullOrEmpty(search))
            {
                var semesters = context.Semesters.Where(u => u.IsDeleted == false && (u.Name.Contains(search))).OrderBy(u => u.Id).ToPagedList(pageNumber, pageSize);
                ViewData["CurrentFilter"] = search;
                return View(semesters);
            }
            else
            {
                var semesters = context.Semesters.Where(u => u.IsDeleted == false).OrderBy(u => u.Id).ToPagedList(pageNumber, pageSize);
                return View(semesters);
            }
        }
        public IActionResult Create()
        {
            Semester semester = new Semester();
            return View(semester);
        }
        [HttpPost]
        public IActionResult Create(Semester semester)
        {
            //check name format
            if (!ValidationHelper.CheckInputNotSpace(semester.Name))
            {
                ViewBag.Message = "Invalid name";
                return View(semester);
            }
            //check date
            if (semester.StartDate > semester.EndDate)
            {
                ViewBag.Message = "Invalid date";
                return View(semester);
            }
            semester.UpdatedAt = DateTime.Now;
            semester.UpdatedBy = HttpContext.Session.GetInt32("UserId");
            context.Semesters.Add(semester);
            context.SaveChanges();
            TempData["Message"] = "Create semester successfully";
            return RedirectToAction("Index");
        }
        public IActionResult Edit(int id)
        {
            Semester semester = context.Semesters.Find(id);
            return View(semester);
        }
        [HttpPost]
        public IActionResult Edit(Semester semester)
        {
            //check name format
            if (!ValidationHelper.CheckInputNotSpace(semester.Name))
            {
                ViewBag.Message = "Invalid name";
                return View(semester);
            }
            //check date
            if (semester.StartDate > semester.EndDate)
            {
                ViewBag.Message = "Invalid date";
                return View(semester);
            }
            Semester updateSemester=context.Semesters.Find(semester.Id);
            updateSemester.Name = semester.Name;
            updateSemester.StartDate = semester.StartDate;
            updateSemester.EndDate = semester.EndDate;
            updateSemester.UpdatedAt = DateTime.Now;
            updateSemester.UpdatedBy = HttpContext.Session.GetInt32("UserId");
            context.Semesters.Update(updateSemester);
            context.SaveChanges();
            TempData["Message"] = "Edit semester successfully";
            return RedirectToAction("Index");
        }
        public IActionResult Delete(int id)
        {
            Semester semester = context.Semesters.Find(id);
            semester.IsDeleted = true;
            semester.UpdatedAt = DateTime.Now;
            semester.UpdatedBy = HttpContext.Session.GetInt32("UserId");
            context.Semesters.Update(semester);
            context.SaveChanges();
            TempData["Message"] = "Delete semester successfully";
            return RedirectToAction("Index");
        }


    }
}
