using SMSProject.Database;
using SMSProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMSProject.Controllers
{
    public class AdminDashboardController : Controller
    {
            SMSdatabase CourseDB = new SMSdatabase();
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
            var admin = CourseDB.GetAdminByCNIC(cnic);
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
        [HttpGet]
            public ActionResult Index(string searchTerm, string department, int? semester, string gender, string activeTab = "students")
            {
            ViewBag.AdminPass = Session["AdminPass"];

           
            // Get all data
            var allStudents = CourseDB.GetAllStudents();
            var allEnrollments = CourseDB.GetAllEnrollments();
            var allRequests = CourseDB.GetPendingSignups();
            var allCourses = CourseDB.GetAllCourses();


            ViewBag.ActiveTab = activeTab;

            // Calculate dashboard counts based on filters
            var enrolledStudents = allEnrollments.Where(e => e.Status == "Enrolled");

            if (!string.IsNullOrEmpty(searchTerm) || !string.IsNullOrEmpty(department) ||
                semester.HasValue || !string.IsNullOrEmpty(gender))
            {
                var filteredCourses = allCourses
                    .Where(c => (string.IsNullOrEmpty(department) || c.Department == department) &&
                      (!semester.HasValue || c.Semester == semester)).ToList();

                var filteredStudents = (from e in enrolledStudents
                                        join s in allStudents on e.StudentID equals s.StudentID
                                        where (string.IsNullOrEmpty(searchTerm) ||
                                               e.StudentID.Contains(searchTerm) ||
                                               s.Name.Contains(searchTerm)) &&
                                              (string.IsNullOrEmpty(department) ||
                                               e.Department == department) &&
                                              (!semester.HasValue || e.Semester == semester) &&
                                              (string.IsNullOrEmpty(gender) || s.Gender == gender)
                                        select new { e, s }).ToList();


                // Count distinct enrollments (based on EnrollmentID)
                ViewBag.TotalEnrollments = filteredStudents.Select(x => x.e.EnrollmentID).Distinct().Count();


                ViewBag.TotalStudents = filteredStudents.Select(x => x.s.StudentID).Distinct().Count();
                ViewBag.TotalCourses = filteredCourses.Count();
            }
            else
            {
                ViewBag.TotalStudents = enrolledStudents.Select(e => e.StudentID).Distinct().Count();
                ViewBag.TotalCourses = allCourses.Count();
                ViewBag.TotalEnrollments = allStudents.Select(e => e.StudentID).Distinct().Count();
            }

            ViewBag.PendingRequests = allRequests.Count(e => e.Status == "Pending");
            ViewBag.ApprovedRequests = allRequests.Count(e => e.Status == "Approved");

            var model = new CourseM();

            switch (activeTab.ToLower())
            {
                case "pending":
                    model.PendingSignupsStd = allRequests.Where(e => e.Status == "Pending").ToList();
                    break;

                case "approved":
                    model.ApprovedSignupsStd = (from signup in allRequests
                                                where signup.Status == "Approved"
                                                join std in allStudents
                                                on signup.StudentID equals std.StudentID
                                                select new Student
                                                {
                                                    StudentID = std.StudentID,
                                                    Gender = std.Gender,
                                                    Email = std.Email,
                                                    Phone = std.Phone,
                                                    Name = std.Name,
                                                    Address = std.Address,
                                                    City = std.City
                                                }).ToList();
                    break;

                default: // students tab
                    var studentsList = (from e in enrolledStudents
                                        join s in allStudents on e.StudentID equals s.StudentID
                                        join c in allCourses on e.CourseID equals c.Id
                                        where (string.IsNullOrEmpty(searchTerm) ||
                                              e.StudentID.Contains(searchTerm) ||
                                              s.Name.Contains(searchTerm)) &&
                                              (string.IsNullOrEmpty(department) ||
                                              c.Department == department) &&
                                              (!semester.HasValue || c.Semester == semester) &&
                                              (string.IsNullOrEmpty(gender) || s.Gender == gender)
                                        group new { e, s, c } by e.StudentID into g
                                        select new Enrollment
                                        {
                                            EnrollmentID = g.First().e.EnrollmentID,
                                            StudentID = g.Key,
                                            StudentName = g.First().s.Name,
                                            Gender = g.First().s.Gender,
                                            Department = g.First().c.Department,
                                            Semester = g.Max(x => x.c.Semester),
                                            EnrollDate = g.First().e.EnrollDate,
                                            Photo= g.First().s.Photo,
                                            CoursesName = string.Join(" || ", g.Select(x => x.c.CourseName)),
                                            Status = g.First().e.Status
                                        }).ToList();
                    model.EnrollList = studentsList;
                    break;
            }

            return View(model);
        }

        public ActionResult UpdateSignupStatus(string studentId, string status)
        {
            if (CourseDB.UpdateSignupStatus(studentId, status))
            {
                TempData["Success"] = $"Student signup {status} successfully";
            }
            else
            {
                TempData["Error"] = "Failed to update status";
            }
            return RedirectToAction("Index", new { activeTab = "pending" });
        }
    }
}

