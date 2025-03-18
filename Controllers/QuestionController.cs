using Edunext.Filters;
using Edunext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Globalization;

namespace Edunext.Controllers
{
    [RoleFilter(1,2)]
    public class QuestionController : Controller
    {
        EdunextContext context = new EdunextContext();
        public IActionResult Details(int id)
        {
            var question = context.Questions
                .Include(q=>q.Comments)
                .ThenInclude(c=>c.User)
                .FirstOrDefault(q => q.Id == id);
            return View(question);
        }
        [RoleFilter(2)]
        [HttpPost]
        public IActionResult Create(IFormFile? file, int classSlotId)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Message"] = "Please select a file to import";
                return RedirectToAction("Details", "ClassSlotContent", new { id = classSlotId });
            }
            ExcelPackage.License.SetNonCommercialOrganization("My Noncommercial organization"); //This will also set the Company property to the organization name provided in the argument.
            List<string> errorList = new List<string>(); // Danh sách lưu lỗi
            int successCount = 0; // Đếm số bản ghi hợp lệ
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
                                var question = new Question();
                                //check valid
                                var content = worksheet.Cells[row, 1].Value;
                                if (content == null || string.IsNullOrWhiteSpace(content.ToString()))
                                {
                                    errorList.Add("Invalid content at row " + row);
                                    continue;
                                }
                                var fromTimeStr = worksheet.Cells[row, 2].GetValue<string>();
                                var fromTime = DateTime.ParseExact(fromTimeStr, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                var toTimeStr = worksheet.Cells[row, 2].GetValue<string>();
                                var toTime = DateTime.ParseExact(fromTimeStr, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                if (fromTime > toTime)
                                {
                                    errorList.Add("Invalid time range at row " + row);
                                    continue;
                                }
                                if (fromTime < System.DateTime.Now)
                                {
                                    errorList.Add("Invalid time range at row " + row);
                                    continue;
                                }
                                question.Content = content.ToString();
                                question.FromTime = fromTime;
                                question.ToTime = toTime;
                                question.ClassSlotId = classSlotId;
                                context.Questions.Add(question);
                                context.SaveChanges();
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
            return RedirectToAction("Details", "ClassSlotContent", new { id = classSlotId });
        }
        // Hàm lưu file lỗi
        private string SaveErrorLog(List<string> errors)
        {
            string errorFileName = $"ImportQuestionErrors_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string errorFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs", errorFileName);

            if (!Directory.Exists(Path.GetDirectoryName(errorFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(errorFilePath));
            }

            System.IO.File.WriteAllLines(errorFilePath, errors);
            return errorFilePath;
        }
        [RoleFilter(2)]
        public IActionResult Edit(int id)
        {
            var question = context.Questions
                .FirstOrDefault(a => a.Id == id);
            return View(question);
        }
        [RoleFilter(2)]
        [HttpPost]
        public IActionResult Edit(Question question)
        {
            if (string.IsNullOrWhiteSpace(question.Content))
            {
                @ViewBag.Message = "Invalid content";
                return View(question);
            }
            if (question.FromTime > question.ToTime)
            {
                @ViewBag.Message = "Invalid time range";
                return View(question);
            }
            if(question.FromTime < System.DateTime.Now)
            {
                @ViewBag.Message = "Invalid time range";
                return View(question);
            }
            var updatedQuestion = context.Questions
                .FirstOrDefault(q => q.Id == question.Id);
            updatedQuestion.Content = question.Content;
            updatedQuestion.Status = question.Status;
            updatedQuestion.FromTime = question.FromTime;
            updatedQuestion.ToTime = question.ToTime;
            updatedQuestion.UpdatedAt = System.DateTime.Now;
            context.Update(updatedQuestion);
            context.SaveChanges();
            TempData["Message"] = "Question updated successfully"; 
            return RedirectToAction("Details", "ClassSlotContent", new { id = question.ClassSlotId });
        }
        [RoleFilter(2)]
        public IActionResult Delete(int id)
        {
            var question = context.Questions
                .FirstOrDefault(q => q.Id == id);
            question.Status = -1;
            context.Update(question);
            context.SaveChanges();
            TempData["Message"] = "Question deleted successfully";
            return RedirectToAction("Details", "ClassSlotContent", new { id = question.ClassSlotId });
        }
    }
}
