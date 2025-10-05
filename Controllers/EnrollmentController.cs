using SMSProject.Database;
using SMSProject.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace SMSProject.Controllers
{
    public class EnrollmentController : Controller
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
        // GET: Courses
        [HttpGet]
        public ActionResult Index(string searchTerm, string department, int? semester, string gender, int page = 1, int pageSize = 10)
        {
            var allStudents = CourseDB.GetAllStudents();
            var allEnrollments = CourseDB.GetAllEnrollments();
            var allCourses = CourseDB.GetAllCourses();

           
            // ViewBag for dropdowns (if any)
            ViewBag.StudentList = new SelectList(allStudents, "StudentID", "StudentID");

            // STEP 1: Apply filters on Enrollment only
            var filteredEnrollments = allEnrollments.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                filteredEnrollments = filteredEnrollments.Where(e =>
                    e.EnrollmentID.Contains(searchTerm) ||
                    e.StudentID.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(department))
            {
                filteredEnrollments = filteredEnrollments
                    .Where(e => allCourses.Any(c => c.Id == e.CourseID && e.Status == "Enrolled" && c.Department == department));
            }
            if (!string.IsNullOrEmpty(gender))
            {
                filteredEnrollments = filteredEnrollments
                    .Where(e => allStudents.Any(c => c.StudentID == e.StudentID && e.Status == "Enrolled" && c.Gender == gender));
            }

            if (semester.HasValue)
            {
                filteredEnrollments = filteredEnrollments
                    .Where(e => allCourses.Any(c => c.Id == e.CourseID && c.Semester == semester.Value && e.Status=="Enrolled"));
            }

            var filteredList = filteredEnrollments.ToList();

            // STEP 1: Filter only 'Enrolled' status courses
            var enrolledCourses = filteredList
                .Where(e => e.Status == "Enrolled")
                .ToList();

            // STEP 2: Join with student and course data
            var joinedList = (from e in enrolledCourses
                              join s in allStudents on e.StudentID equals s.StudentID
                              join c in allCourses on e.CourseID equals c.Id
                              select new
                              {
                                  e.EnrollmentID,
                                  e.StudentID,
                                  s.Name,
                                  s.Gender,
                                  c.Department,
                                  c.Semester,
                                  e.EnrollDate,
                                  c.CourseName,
                                  e.Status
                              }).ToList();

            // STEP 3: Group by StudentID and combine enrolled course names
            var groupedList = joinedList
                .GroupBy(j => j.StudentID)
                .Select(g => new Enrollment
                {
                    EnrollmentID = g.First().EnrollmentID,
                    StudentID = g.Key,
                    StudentName = g.First().Name,
                    Gender = g.First().Gender,
                    Department = g.First().Department,
                    Semester = g.Max(x => x.Semester), // Optional
                    EnrollDate = g.First().EnrollDate,
                    CoursesName = string.Join(" || ", g.Select(x => x.CourseName)),
                    Status = "Enrolled"
                }).ToList();

            // STEP 4: Pagination
            int totalCount = groupedList.Count();
            var pagedData = groupedList
                .OrderBy(e => e.EnrollmentID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new CourseM
            {
                EnrollList = pagedData,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return View(model);
        }




        [HttpPost]
        public ActionResult Add(string EnrollmentID, string StudentID, string department)
        {
            SMSdatabase db = new SMSdatabase();
            int semester = 1;
            DateTime date = DateTime.Now;

            var allStudents = db.GetAllStudents();
            ViewBag.StudentList = new SelectList(allStudents, "StudentID", "StudentID");

            // ❗ Check if the EnrollmentID already exists
            bool idExists = db.GetAllEnrollments().Any(e => e.EnrollmentID == EnrollmentID);
            if (idExists)
            {
                TempData["Error"] = "This Enrollment ID already exists. Please use a unique Enrollment ID.";
                TempData["ShowAddModal"] = true;
                return RedirectToAction("Index");
            }

            // ❗ Check if the student is already enrolled in semester 1
            var existingEnrollments = db.GetAllEnrollments()
                .Where(e => e.StudentID == StudentID && e.Semester == semester)
                .ToList();

            if (existingEnrollments.Any())
            {
                TempData["Error1"] = "This student is already enrolled in Semester 1.";
                TempData["ShowAddModal"] = true;
                return RedirectToAction("Index");
            }

            // ❗ Get all semester 1 courses of the selected department
            var semesterCourses = db.GetAllCourses()
                .Where(c => c.Department == department && c.Semester == semester)
                .ToList();

            List<string> fullCourses = new List<string>();  // To store courses that are already full

            foreach (var course in semesterCourses)
            {
                // Count how many students are currently enrolled in this course
                int currentEnrollmentCount = db.GetAllEnrollments()
                    .Count(e => e.CourseID == course.Id && e.Status == "Enrolled");

                if (currentEnrollmentCount >= course.Capacity)
                {
                    fullCourses.Add(course.CourseName);
                    continue;  // Skip this course, it's full
                }

                // ✅ Proceed to enroll the student in this course
                db.InsertEnrollment(new Enrollment
                {
                    EnrollmentID = EnrollmentID,
                    StudentID = StudentID,
                    CourseID = course.Id,
                    Department = department,
                    EnrollDate = date,
                    Semester = semester,
                    CoursesName = course.CourseName,
                    Status = "Enrolled"
                });
            }

            if (fullCourses.Any())
            {
                TempData["Warning"] = "Some courses were full and the student could not be enrolled in them: " + string.Join(", ", fullCourses);
            }
            else
            {
                TempData["Success"] = "Student enrolled successfully in all semester courses.";
            }

            return RedirectToAction("Index");
        }




        [HttpPost]
        public ActionResult Delete(string id)
        {
            bool result = CourseDB.DeleteEnrollment(id);
            if (result)
            {
                TempData["Message"] = "This Enrollment deleted successfully!";
            }
            else
            {
                TempData["Message"] = "Delete failed!";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult ViewEnrollment(string id, int semester)
        {
            var allStudents = CourseDB.GetAllStudents();
            var allEnrollments = CourseDB.GetAllEnrollments();
            var allCourses = CourseDB.GetAllCourses();

            var failedCourses = allEnrollments
                .Where(e => e.Status == "Failed" && e.EnrollmentID == id)
                .ToList();

            if (failedCourses.Any())
            {
                ViewBag.FailedCourses = new CourseM { EnrollList = failedCourses };
            }
            else
            {
                ViewBag.FailedCourses = null;
            }

            var filteredEnrollments = allEnrollments
                .Where(e => e.EnrollmentID == id && e.Semester == semester)
                .ToList();

            if (!filteredEnrollments.Any())
            {
                // ✅ Instead of 404, pass empty model with TempData message
                TempData["Info"] = "No enrolled courses found for this semester.";
                return View(new CourseM { EnrollList = new List<Enrollment>() });
            }

            var joinedList = (from e in filteredEnrollments
                              join s in allStudents on e.StudentID equals s.StudentID
                              join c in allCourses on e.CourseID equals c.Id
                              select new Enrollment
                              {
                                  EnrollmentID = e.EnrollmentID,
                                  StudentID = e.StudentID,
                                  StudentName = s.Name,
                                  Photo = s.Photo,
                                  Gender = s.Gender,
                                  CourseID = e.CourseID,
                                  Department = c.Department,
                                  Semester = c.Semester,
                                  EnrollDate = e.EnrollDate,
                                  CoursesName = c.CourseName,
                                  Status = e.Status
                              }).ToList();

            return View(new CourseM { EnrollList = joinedList });
        }

        [HttpPost]
        public ActionResult PromoteStudent(string EnrollmentID, List<string> FailedCourses)
        {
            SMSdatabase db = new SMSdatabase();
            DateTime date = DateTime.Now;

            var latestEnroll = db.GetAllEnrollments().Where(e => e.EnrollmentID == EnrollmentID)
                              .OrderByDescending(e => e.Semester)
                              .FirstOrDefault();

            if (latestEnroll == null)
            {
                TempData["Error"] = "Enrollment record not found.";
                return RedirectToAction("Index");
            }

            string studentID = latestEnroll.StudentID;
            string department = latestEnroll.Department;
            int currentSemester = latestEnroll.Semester;


            var currentEnrollments = db.GetAllEnrollments()
                .Where(e => e.StudentID == studentID && e.Semester == currentSemester)
                .ToList();

            bool hasEnrolledCourses = currentEnrollments.Any(e => e.Status == "Enrolled");
            if (hasEnrolledCourses)
            {
                TempData["Error1"] = "Student cannot be promoted. Some courses are still marked as 'Enrolled'. Please update course statuses.";
                return RedirectToAction("ViewEnrollment", new { id = EnrollmentID, semester = currentSemester });
            }

            int nextSemester = currentSemester + 1;

            // ✅ Assign NEW semester courses (standard flow)
            var nextCourses = db.GetAllCourses()
                .Where(c => c.Semester == nextSemester && c.Department == department)
                .ToList();

            foreach (var course in nextCourses)
            {
                db.InsertEnrollment(new Enrollment
                {
                    EnrollmentID = EnrollmentID,
                    StudentID = studentID,
                    CourseID = course.Id,
                    Department = department,
                    Semester = nextSemester,
                    CoursesName = course.CourseName,
                    EnrollDate = date,
                    Status = "Enrolled"
                });
            }

            // ✅ NOW → Assign *failed* courses → ALSO IN nextSemester (NOT currentSemester)
            if (FailedCourses != null && FailedCourses.Any())
            {
                var failedCourseIds = FailedCourses.Select(int.Parse).ToList();

                var failedCoursesInfo = db.GetAllCourses()
                    .Where(c => failedCourseIds.Contains(c.Id))
                    .ToList();

                foreach (var course in failedCoursesInfo)
                {
                    db.InsertEnrollment(new Enrollment
                    {
                        EnrollmentID = EnrollmentID,
                        StudentID = studentID,
                        CourseID = course.Id,
                        Department = course.Department,
                        Semester = nextSemester,   // ✅ Must be nextSemester NOT currentSemester
                        CoursesName = course.CourseName,
                        EnrollDate = date,
                        Status = "Enrolled"
                    });
                }
            }

            TempData["Success"] = $"Student promoted successfully to Semester {nextSemester}!";
            return RedirectToAction("Index");
        }


        [HttpPost]
        public ActionResult UpdateStatus(string EnrollmentID, string CoursesName,int Semester, string Status)
        {
            bool updated = CourseDB.UpdateStatus(EnrollmentID, CoursesName, Status);

            if (updated)
                TempData["Success3"] = "Status updated successfully!";
            else
                TempData["Error3"] = "Update failed.";

            return RedirectToAction("ViewEnrollment", new { id = EnrollmentID , semester = Semester });
        }

       


        private readonly SMSdatabase db = new SMSdatabase();
        [HttpGet]
        public JsonResult GetCourseById(int id)
        {
            var courses = CourseDB.GetAllCourses();
            var course = courses
                           .Where(c => c.Id == id)
                           .Select(c => new
                           {
                               c.Id,
                               c.CourseCode,
                               c.CourseName,
                               c.Department,
                               c.Credits,
                               c.Instructor,
                               c.Semester,
                               c.Capacity,
                               c.Description
                           })
                           .FirstOrDefault();

            return Json(course, JsonRequestBehavior.AllowGet);
        }
    }
}