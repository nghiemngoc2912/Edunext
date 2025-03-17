using Edunext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using X.PagedList.Extensions;

namespace Edunext.Controllers
{
    public class SlotController : Controller
    {
        EdunextContext context = new EdunextContext();
        public IActionResult Index(int courseId, int? page)
        {
            int pageSize = 5; // Số item trên mỗi trang
            int pageNumber = (page ?? 1);
            //check if form is submitted
            string search = Request.Query["searchString"];
            ViewBag.CourseId = courseId;
            if (!string.IsNullOrEmpty(search))
            {
                var slots = context.Slots.Where(s => s.CourseId == courseId && s.Name.Contains(search) && s.IsDeleted == false).ToPagedList(pageNumber, pageSize);
                ViewData["CurrentFilter"] = search;
                return View(slots);
            }
            else
            {
                var slots = context.Slots.Where(s => s.CourseId == courseId && s.IsDeleted == false).OrderBy(u => u.Order).ToPagedList(pageNumber, pageSize);
                return View(slots);
            }
        }
        [HttpPost]
        public IActionResult Create(IFormFile? file,int courseId)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Message"] = "Please select a file to import";
                return RedirectToAction("Index", new {courseId=courseId});
            }
            ExcelPackage.License.SetNonCommercialOrganization("My Noncommercial organization"); //This will also set the Company property to the organization name provided in the argument.
            int successCount = 0; // Đếm số bản ghi hợp lệ
            List<string> errorList = new List<string>(); // Danh sách lưu lỗi
            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // Lấy sheet đầu tiên
                    int rowCount = worksheet.Dimension.Rows; // Số dòng
                    using (var transaction = context.Database.BeginTransaction()) // Bắt đầu transaction
                    {
                        try
                        {
                            for (int row = 2; row <= rowCount; row++) // Bỏ qua tiêu đề (hàng 1)
                            {
                                int order = Int32.Parse(worksheet.Cells[row, 1].Value?.ToString()?.Trim());
                                if (context.Slots.Any(s=>s.CourseId==courseId&&s.Order==order))
                                {
                                    errorList.Add($"Row {row}: Order exist.");
                                    continue;
                                }
                                
                                string name = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                                if (string.IsNullOrWhiteSpace(name))
                                {
                                    errorList.Add($"Row {row}: Name is missing.");
                                    continue;
                                }
                                Slot slot = new Slot
                                {
                                    Order = order,
                                    Name = name,
                                    CourseId=courseId,
                                    UpdatedAt = DateTime.Now,
                                    UpdatedBy = HttpContext.Session.GetInt32("UserId")
                                };
                                context.Slots.Add(slot);
                                successCount++;
                            }
                            if (errorList.Count > 0)
                            {
                                transaction.Rollback();
                                string errorFilePath = SaveErrorLog(errorList);
                                System.Diagnostics.Process.Start("notepad.exe", errorFilePath); // Mở file lỗi
                                TempData["Message"] = $"Import failed! {errorList.Count} errors found. Check the error log.";
                            }
                            else
                            {
                                context.SaveChanges(); // Lưu vào database
                                transaction.Commit(); // Xác nhận lưu
                                TempData["Message"] = $"Import successfully! {successCount} records added.";

                            }
                    }
                        catch (Exception ex)
                        {
                        transaction.Rollback(); // Hủy bỏ nếu có lỗi bất ngờ
                        errorList.Add($"Unexpected error: {ex.Message}");
                        string errorFilePath = SaveErrorLog(errorList);
                        System.Diagnostics.Process.Start("notepad.exe", errorFilePath); // Mở file lỗi
                        TempData["Message"] = $"Import failed due to unexpected error. Check the error log.";
                    }
                }
                }
            }
            return RedirectToAction("Index",new { courseId = courseId });
        }
        private string SaveErrorLog(List<string> errors)
        {
            string errorFileName = $"ImportSlotErrors_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string errorFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs", errorFileName);

            if (!Directory.Exists(Path.GetDirectoryName(errorFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(errorFilePath));
            }

            System.IO.File.WriteAllLines(errorFilePath, errors);
            return errorFilePath;
        }
        public IActionResult Edit(int id)
        {
            Slot slot = context.Slots.FirstOrDefault(s => s.Id == id);
            if (slot == null)
            {
                TempData["Message"] = "Slot not found";
                return RedirectToAction("Index");
            }
            else
            {
                return View(slot);
            }
        }
        [HttpPost]
        public IActionResult Edit(Slot slot)
        {
            var name = slot.Name;
            //check order is number from 1 to 100
            if (slot.Order < 1)
            {
                ViewBag.Message= "Order must be a number greater than";
                return View(slot);
            }//check order exist
            else if (context.Slots.Any(s => s.CourseId == slot.CourseId && s.Order == slot.Order))
            {
                ViewBag.Message = "Order exist";
                return View(slot);
            } else if (string.IsNullOrWhiteSpace(name))
            {
                ViewBag.Message = "Name is required";
                return View(slot);
            }
            else
            {
                Slot slotInDb = context.Slots.FirstOrDefault(s => s.Id == slot.Id);
                if (slotInDb == null)
                {
                    ViewBag.Message = "Slot not found";
                    return RedirectToAction("Index", new { courseId = slot.CourseId });
                }
                else
                {
                    slotInDb.Order = slot.Order;
                    slotInDb.Name = slot.Name;
                    slotInDb.UpdatedAt = DateTime.Now;
                    slotInDb.UpdatedBy = HttpContext.Session.GetInt32("UserId");
                    context.Slots.Update(slotInDb);
                    context.SaveChanges();
                    return RedirectToAction("Index", new { courseId = slot.CourseId });
                }
            }
        }
        public IActionResult Delete(int id,int courseId)
        {
            Slot slot=context.Slots.FirstOrDefault(s=>s.Id == id);
            if (slot == null)
            {
                TempData["Message"] = "Slot not found";
                return RedirectToAction("Index", new { courseId = courseId });
            }
            else
            {
                slot.IsDeleted = true;
                context.Slots.Update(slot);
                context.SaveChanges();
                return RedirectToAction("Index", new { courseId= slot.CourseId });
            }
        }

        public IActionResult NormIndex(int classId, int? page)
        {
            var course=context.Classrooms.Select(cl=>cl.Course).FirstOrDefault(c=>c.Id==classId);
            var courseId = course.Id;
            int pageSize = 5; // Số item trên mỗi trang
            int pageNumber = (page ?? 1);
            //check if form is submitted
            string search = Request.Query["searchString"];
            ViewBag.classId = classId;
            if (!string.IsNullOrEmpty(search))
            {
                var slots = context.Slots.Where(s => s.CourseId == courseId && s.Name.Contains(search) && s.IsDeleted == false).ToPagedList(pageNumber, pageSize);
                ViewData["CurrentFilter"] = search;
                return View(slots);
            }
            else
            {
                var slots = context.Slots.Where(s => s.CourseId == courseId && s.IsDeleted == false).OrderBy(u => u.Order).ToPagedList(pageNumber, pageSize);
                return View(slots);
            }
        }



    }
}
