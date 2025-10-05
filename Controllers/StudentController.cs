using SMSProject.Database;
using SMSProject.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMSProject.Controllers
{
    public class StudentController : Controller
    {
        SMSdatabase StudentDB = new SMSdatabase();
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            ViewBag.AdminCNIC = Session["AdminCNIC"];
            string cnic = Session["AdminCNIC"]?.ToString();

            if (string.IsNullOrEmpty(cnic))
            {
                TempData["Error"] = "Please login to access the dashboard";
                filterContext.Result = RedirectToAction("AdminLogin", "Admin");
                return;
            }
            var admin = StudentDB.GetAdminByCNIC(cnic);
            // Convert photo to Base64 string
            if (admin.Photo != null && admin.Photo.Length > 0)
            {
                string base64Image = Convert.ToBase64String(admin.Photo);
                ViewBag.AdminPhoto = $"data:image/jpeg;base64,{base64Image}";
            }
            else
            {
                ViewBag.AdminPhoto = Url.Content("~/Content/Images/default-profile.png"); // Default image if none
            }

            ViewBag.AdminName = admin.Name;
            ViewBag.AdminEmail = admin.Email;
            ViewBag.AdminPhone = admin.Phone;
            ViewBag.AdminCity = admin.City;
        }
        // GET: Courses
        [HttpGet]

        public ActionResult Index(string searchTerm, string gender, int page = 1, int pageSize = 10)
        {
            ViewBag.insertSu = TempData["Success"];

            ViewBag.insertFa = TempData["Error"];
            
            var allStudent = StudentDB.GetAllStudents(); 
            if (!string.IsNullOrEmpty(searchTerm))
                allStudent = allStudent.Where(c =>
                    c.StudentID.Contains(searchTerm) ||
                     c.CNIC.Contains(searchTerm) ||
                    c.Name.Contains(searchTerm)).ToList();
            if (!string.IsNullOrEmpty(gender))
                allStudent = allStudent.Where(c => c.Gender == gender).ToList();


            int totalCount = allStudent.Count();
            var pagedCourses = allStudent
                .OrderBy(c => c.StudentID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new CourseM
            {
                StudentList = pagedCourses,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return View(model);
        }


        [HttpPost]
        public ActionResult Add(CourseM model, HttpPostedFileBase PhotoFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (PhotoFile != null && PhotoFile.ContentLength > 0)
                    {
                        // Validate file type
                        string[] allowedTypes = { "image/jpeg", "image/png", "image/gif" };
                        if (!allowedTypes.Contains(PhotoFile.ContentType))
                        {
                            ModelState.AddModelError("Photo", "Invalid file type. Only JPG, PNG and GIF are allowed.");
                            return RedirectToAction("Index");
                        }

                        // Validate file size (e.g., max 2MB)
                        if (PhotoFile.ContentLength > 2 * 1024 * 1024)
                        {
                            ModelState.AddModelError("Photo", "File size cannot exceed 2MB.");
                            return RedirectToAction("Index");
                        }

                        using (var binaryReader = new BinaryReader(PhotoFile.InputStream))
                        {
                            model.NewStudent.Photo = binaryReader.ReadBytes(PhotoFile.ContentLength);
                        }
                    }

                    SMSdatabase db = new SMSdatabase();

                    bool result = db.InsertStudent(model.NewStudent);
                    if (result)
                    {
                        TempData["Success"] = "Student added successfully!";
                    }
                    else
                    {
                        TempData["Error"] = "Failed to add student. This Student already exists.";
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    // Log the exception details
                    TempData["Error"] = "This Student already exists.";

                    // You might want to log the actual error: ex.Message
                }
            }
            else
            {
                TempData["Error"] = "Please check the form data and try again.";
            }

            return RedirectToAction("Index");
        }
        public ActionResult Search(string searchTerm)
        {
            var students = StudentDB.GetAllStudents();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                students = students.Where(c =>
                    c.StudentID.ToLower().Contains(searchTerm) ||
                    c.Name.ToLower().Contains(searchTerm) ||
                     c.CNIC.ToLower().Contains(searchTerm)
                ).ToList();
            }

            var viewModel = new CourseM
            {
                NewStudent = new Student(),
                StudentList = students
            };

            return RedirectToAction("Index", viewModel);
        }
        [HttpPost]
        public ActionResult Update(Student student, HttpPostedFileBase PhotoFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get existing student to preserve photo if no new one is uploaded
                    var existingStudent = StudentDB.GetAllStudents()
                        .FirstOrDefault(s => s.StudentID == student.StudentID);

                    if (existingStudent == null)
                    {
                        TempData["Error"] = "Student not found.";
                        return RedirectToAction("Index");
                    }

                    // Only update photo if a new one is uploaded
                    if (PhotoFile != null && PhotoFile.ContentLength > 0)
                    {
                        string[] allowedTypes = { "image/jpeg", "image/png", "image/gif" };
                        if (!allowedTypes.Contains(PhotoFile.ContentType))
                        {
                            TempData["Error1"] = "Invalid file type. Only JPG, PNG and GIF are allowed.";
                            return RedirectToAction("Index");
                        }

                        if (PhotoFile.ContentLength > 2 * 1024 * 1024)
                        {
                            TempData["Error1"] = "File size cannot exceed 2MB.";
                            return RedirectToAction("Index");
                        }

                        using (var binaryReader = new BinaryReader(PhotoFile.InputStream))
                        {
                            student.Photo = binaryReader.ReadBytes(PhotoFile.ContentLength);
                        }
                    }
                    else
                    {
                        // Keep existing photo if no new one is uploaded
                        student.Photo = existingStudent.Photo;
                    }
                     
                    bool result = StudentDB.UpdateStudent(student);
                    if (result)
                    {
                        TempData["Success"] = "Student updated successfully!";
                    }
                    else
                    {
                        TempData["Error"] = "Failed to update student.";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    TempData["Error"] = "An error occurred while updating the student.";
                    // Log the exception: ex.Message
                }
            }
            else
            {
                TempData["Error"] = "Please check the form data and try again.";
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult Delete(string id)
        {
            bool result = StudentDB.DeleteStudent(id);
            if (result)
            {
                TempData["Message"] = "Student deleted successfully!";
            }
            else
            {
                TempData["Message"] = "Delete failed!";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public JsonResult GetStudentById(string id)
        {
            try
            {
                var student = StudentDB.GetAllStudents()
                    .FirstOrDefault(s => s.StudentID == id);

                if (student == null)
                {
                    return Json(new { success = false, message = "Student not found" }, JsonRequestBehavior.AllowGet);
                }

                var studentData = new
                {
                    student.StudentID,
                    student.Name,
                    student.CNIC,
                    student.Address,
                    student.City,
                    student.Gender,
                    student.Email,
                    student.Phone,
                    PhotoUrl = student.Photo != null ?
                        $"data:image/jpeg;base64,{Convert.ToBase64String(student.Photo)}" :
                        "/Content/Images/default-profile.png"
                };

                return Json(new { success = true, data = studentData }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Json(new { success = false, message = "Error retrieving student details" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
       