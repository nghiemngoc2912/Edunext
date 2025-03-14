using Edunext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using X.PagedList.Extensions;

namespace Edunext.Controllers
{
    public class ClassroomController : Controller
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
                var classes = context.Classrooms.Include(c => c.Course).Include(c => c.Teacher).Include(c => c.Semester).Where(c => (c.IsDeleted == false)&&(c.Name.Contains(search))).OrderBy(u => u.Id).ToPagedList(pageNumber, pageSize);
                ViewData["CurrentFilter"] = search;
                return View(classes);
            }
            else 
            {
                var classes = context.Classrooms.Include(c => c.Course).Include(c => c.Teacher).Include(c => c.Semester).Where(c => c.IsDeleted == false).OrderBy(u => u.Id).ToPagedList(pageNumber, pageSize);
                return View(classes);
            }
            
        }
        public IActionResult NormIndex(int? page,int? userId) {
            int pageSize = 5; // Số item trên mỗi trang
            int pageNumber = (page ?? 1);
            int role= (int)HttpContext.Session.GetInt32("Role");
            if (role==2)
            {
                var classes = context.Classrooms.Include(c => c.Course).Include(c => c.Teacher).Include(c => c.Semester).Where(c => (c.IsDeleted == false)&&(c.TeacherId==userId)).OrderBy(u => u.Id).ToPagedList(pageNumber, pageSize);
                return View(classes);
            }
            else
            {
                var classes = context.ClassEnrollments
                    .Include(c => c.Class)
                        .ThenInclude(cl => cl.Course) // Include Course của Class
                    .Include(c => c.Class)
                        .ThenInclude(cl => cl.Semester) // Include Semester của Class
                    .Include(c => c.Class)
                        .ThenInclude(cl => cl.Teacher) // Include Teacher của Class
                    .Where(c => c.IsDeleted==false && c.UserId == userId)
                    .Select(c => c.Class)
                    .OrderBy(c => c.Id)
                    .ToPagedList(pageNumber, pageSize);

                return View(classes);
            }
        }
        [HttpPost]
        public IActionResult CreateClasses(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Message"] = "Please select a file to import";
                return RedirectToAction("Index");
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
                                string className = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                                if (string.IsNullOrEmpty(className))
                                {
                                    errorList.Add($"Row {row}: Class Name is missing.");
                                    continue;
                                }
                                string courseCode = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                                if (string.IsNullOrEmpty(courseCode))
                                {
                                    errorList.Add($"Row {row}: Course Code is missing.");
                                    continue;
                                }
                                Course course = context.Courses.FirstOrDefault(c => c.Code == courseCode);
                                if (course == null)
                                {
                                    errorList.Add($"Row {row}: Course Code {courseCode} is not found.");
                                    continue;
                                }
                                string semesterName = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                                if (string.IsNullOrEmpty(semesterName))
                                {
                                    errorList.Add($"Row {row}: Semester Name is missing.");
                                    continue;
                                }
                                Semester semester = context.Semesters.FirstOrDefault(c => c.Name == semesterName);
                                if (semester == null)
                                {
                                    errorList.Add($"Row {row}: Semester Code {semesterName} is not found.");
                                    continue;
                                }
                                string teacherCode = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                                if (string.IsNullOrEmpty(teacherCode))
                                {
                                    errorList.Add($"Row {row}: Teacher Code is missing.");
                                    continue;
                                }
                                User teacher = context.Users.FirstOrDefault(c => c.Code == teacherCode&&c.Role==2);
                                if (teacher == null)
                                {
                                    errorList.Add($"Row {row}: Teacher Code {teacherCode} is not found.");
                                    continue;
                                }
                                //check if class exist with same class name, course, semester
                                var classExist = context.Classrooms.FirstOrDefault(c => c.Name == className && c.CourseId == course.Id && c.SemesterId == semester.Id);
                                if (classExist != null)
                                {
                                    errorList.Add($"Row {row}: Class {className} already exists.");
                                    continue;
                                }
                                Classroom classroom = new Classroom
                                {
                                    Name = className,
                                    CourseId = course.Id,
                                    SemesterId = semester.Id,
                                    TeacherId = teacher.Id,
                                    UpdatedAt = DateTime.Now,
                                    UpdatedBy = HttpContext.Session.GetInt32("UserId")
                                };
                                context.Classrooms.Add(classroom);
                                successCount++;
                            }
                            if(errorList.Count > 0)
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
            return RedirectToAction("Index");
        }
        private string SaveErrorLog(List<string> errors)
        {
            string errorFileName = $"ImportClassErrors_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
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
            var classroom = context.Classrooms
                .Include(c=>c.Course)
                .Include(c=>c.Teacher)
                .Include(c=>c.Semester)
                .FirstOrDefault(c => c.Id == id);
            if (classroom == null)
            {
                TempData["Message"] = "No classroom found";
                return RedirectToAction("Index");
            }
            else
            {
                this.ViewBag.Teachers = context.Users.Where(u => u.Role == 2&&u.IsDeleted==false);
                this.ViewBag.Courses = context.Courses.Where(c => c.IsDeleted == false);
                this.ViewBag.Semesters = context.Semesters.Where(c => c.IsDeleted == false);
                return View(classroom);
            }
        }
        [HttpPost]
        public IActionResult Edit(Classroom c)
        {

            this.ViewBag.Teachers = context.Users.Where(u => u.Role == 2 && u.IsDeleted == false);
            this.ViewBag.Courses = context.Courses.Where(c => c.IsDeleted == false);
            this.ViewBag.Semesters = context.Semesters.Where(c => c.IsDeleted == false);
            //check format
            //check courseId, teacherId, semesterId exist
            string className = c.Name;
            var course=context.Courses.FirstOrDefault(e => e.Id == c.CourseId && e.IsDeleted == false);
            var semester=context.Semesters.FirstOrDefault(e => e.Id == c.SemesterId && e.IsDeleted == false);
            var teacher=context.Users.FirstOrDefault(t => t.Id == c.TeacherId && t.Role == 2 && t.IsDeleted == false);
            if (string.IsNullOrEmpty(className))
            {
                ViewBag.Message = "Class Name is missing.";
                return View(c);
            }
            else if (course == null)
            {
                ViewBag.Message = "Course not found.";
                return View(c);
            } else if (teacher == null)
            {
                ViewBag.Message = "Teacher not found.";
                return View(c);
            }
            else if (semester == null)
            {
                ViewBag.Message = "Semester not found.";
                return View(c);
            } else if (context.Classrooms.FirstOrDefault(cl => cl.Name == className && cl.CourseId == course.Id && cl.SemesterId == semester.Id) != null)
            {
                ViewBag.Message = "Class exists.";
                return View(c);
            }
            else
            {
                Classroom updateClass=context.Classrooms.FirstOrDefault(cl => cl.Id == c.Id);
                if(updateClass==null)
                {
                    TempData["Message"] = "No classroom found";
                    return RedirectToAction("Index");
                }
                updateClass.Name = c.Name;
                updateClass.CourseId = c.CourseId;
                updateClass.TeacherId = c.TeacherId;
                updateClass.SemesterId = c.SemesterId;
                updateClass.UpdatedAt = DateTime.Now;
                updateClass.UpdatedBy = HttpContext.Session.GetInt32("UserId");
                context.Classrooms.Update(updateClass);
                context.SaveChanges();
                TempData["Message"] = "Update classroom successfully";
                return RedirectToAction("Index");
            }
        }



        public IActionResult Delete(int id)
        {
            Classroom classroom = context.Classrooms.Find(id);
            classroom.IsDeleted = true;
            classroom.UpdatedAt = DateTime.Now;
            classroom.UpdatedBy = HttpContext.Session.GetInt32("UserId");
            context.Classrooms.Update(classroom);
            context.SaveChanges();
            TempData["Message"] = "Delete classroom successfully";
            return RedirectToAction("Index");
        }
    }
}
