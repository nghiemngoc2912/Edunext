using Edunext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using X.PagedList.Extensions;

namespace Edunext.Controllers
{
    public class ClassEnrollmentController : Controller
    {
        EdunextContext context = new EdunextContext();
        public IActionResult Index(int? classroomId,int? page)
        {
            int pageSize = 5; // Số item trên mỗi trang
            int pageNumber = (page ?? 1);
            string search = Request.Query["searchString"];
            if (!string.IsNullOrEmpty(search))
            {
                var classEnrollments = context.ClassEnrollments.Include(c => c.User).Where(c => (c.IsDeleted == false) && c.ClassId==classroomId && (c.User.FirstName.Contains(search)||c.User.LastName.Contains(search)||c.User.Email.Contains(search))).OrderBy(u => u.Id).ToPagedList(pageNumber, pageSize);
                ViewData["CurrentFilter"] = search;
                return View(classEnrollments);
            }
            else
            {
                var classEnrollments = context.ClassEnrollments.Include(c => c.User).Where(c => (c.IsDeleted == false) && c.ClassId == classroomId).OrderBy(u => u.Id).ToPagedList(pageNumber, pageSize);
                return View(classEnrollments);
            }
        }

        [HttpPost]
        public IActionResult CreateEnrollments(IFormFile? file, int classroomId)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Message"] = "Please select a file to import";
                return RedirectToAction("Index");
            }

            ExcelPackage.License.SetNonCommercialOrganization("My Noncommercial organization"); //This will also set the Company property to the organization name provided in the argument.

            List<string> errorList = new List<string>(); // Danh sách lưu lỗi
            List<string> excelUserCodes = new List<string>(); // Kiểm tra trùng lặp trong file Excel
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
                                string userCode = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                                if (string.IsNullOrEmpty(userCode))
                                {
                                    errorList.Add($"Row {row}: User Code is missing.");
                                    continue;
                                }

                                // Kiểm tra trùng lặp trong file Excel
                                if (excelUserCodes.Contains(userCode))
                                {
                                    errorList.Add($"Row {row}: User Title {userCode} is duplicated in the Excel file.");
                                    continue;
                                }
                                excelUserCodes.Add(userCode);

                                // Kiểm tra user có tồn tại trong DB không
                                User user = context.Users.FirstOrDefault(u => u.Code == userCode);
                                if (user == null)
                                {
                                    errorList.Add($"Row {row}: User with code {userCode} does not exist.");
                                    continue;
                                }

                                // Kiểm tra user có bị trùng trong DB không
                                bool alreadyEnrolled = context.ClassEnrollments.Any(c => c.UserId == user.Id && c.ClassId == classroomId);
                                if (alreadyEnrolled)
                                {
                                    errorList.Add($"Row {row}: User with code {userCode} is already enrolled.");
                                    continue;
                                }

                                // Thêm user vào lớp học
                                ClassEnrollment classEnrollment = new ClassEnrollment
                                {
                                    UserId = user.Id,
                                    ClassId = classroomId,
                                    UpdatedAt = DateTime.Now,
                                    UpdatedBy = HttpContext.Session.GetInt32("UserId")
                                };
                                context.ClassEnrollments.Add(classEnrollment);
                                successCount++;
                            }

                            // Nếu có lỗi, rollback và không import gì cả
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
            return RedirectToAction("Index", new { classroomId = classroomId });
        }

        // Hàm lưu file lỗi
        private string SaveErrorLog(List<string> errors)
        {
            string errorFileName = $"ImportClassEnrollmentErrors_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string errorFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs", errorFileName);

            if (!Directory.Exists(Path.GetDirectoryName(errorFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(errorFilePath));
            }

            System.IO.File.WriteAllLines(errorFilePath, errors);
            return errorFilePath;
        }


        public IActionResult Delete(int id)
        {
            ClassEnrollment classEnrollment = context.ClassEnrollments.Find(id);
            classEnrollment.IsDeleted = true;
            classEnrollment.UpdatedAt = DateTime.Now;
            classEnrollment.UpdatedBy = HttpContext.Session.GetInt32("UserId");
            context.ClassEnrollments.Update(classEnrollment);
            context.SaveChanges();
            TempData["Message"] = "Unenroll student successfully";
            return RedirectToAction("Index", new { classroomId = classEnrollment.ClassId });
        }
    }
}
