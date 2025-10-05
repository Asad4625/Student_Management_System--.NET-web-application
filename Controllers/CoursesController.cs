using SMSProject.Database;
using SMSProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMSProject.Controllers
{
    public class CoursesController : Controller
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
        public ActionResult Index(string searchTerm, string department, int? semester, int page = 1, int pageSize = 10)
        {
            var allCourses = CourseDB.GetAllCourses();
            var filteredCourses = allCourses.AsQueryable();

            // Apply department filter
            if (!string.IsNullOrEmpty(department))
            {
                filteredCourses = filteredCourses.Where(c => c.Department == department);
            }

            // Apply semester filter independently
            if (semester.HasValue && semester > 0)
            {
                filteredCourses = filteredCourses.Where(c => c.Semester == semester.Value);
            }

            // Apply search term filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                filteredCourses = filteredCourses.Where(c =>
                    c.CourseCode.ToLower().Contains(searchTerm) ||
                    c.CourseName.ToLower().Contains(searchTerm) ||
                    c.Department.ToLower().Contains(searchTerm));
            }

            // Calculate pagination
            int totalCount = filteredCourses.Count();
            var pagedCourses = filteredCourses
                .OrderBy(c => c.Department)
                .ThenBy(c => c.Semester)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new CourseM
            {
                CourseList = pagedCourses,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                PageSize = pageSize,
                TotalCount = totalCount
            };

            // Add selected values to ViewBag
            ViewBag.SelectedDepartment = department;
            ViewBag.SelectedSemester = semester;
            ViewBag.SearchTerm = searchTerm;

            return View(model);
        }

        [HttpPost]
        public ActionResult Add(CourseM model)
        {
            if (ModelState.IsValid)
            {
                SMSdatabase db = new SMSdatabase();
                bool result = db.InsertCourse(model.NewCourse);

                if (result)
                {
                    ViewBag.Message = "Course added successfully!";
                }
                else
                {
                    ViewBag.Message = "Something went wrong!";
                }
            }

           
            return RedirectToAction("Index");
        }
        public ActionResult Search(string searchTerm)
        {
            var courses = CourseDB.GetAllCourses();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                courses = courses.Where(c =>
                    c.CourseCode.ToLower().Contains(searchTerm) ||
                    c.CourseName.ToLower().Contains(searchTerm)
                ).ToList();
            }

            var viewModel = new CourseM
            {
                NewCourse = new Course(),
                CourseList = courses
            };

            return RedirectToAction("Index",viewModel);
        }

        [HttpPost]
        public ActionResult Update(Course course)
        {
            if (ModelState.IsValid)
            {
                bool result = CourseDB.UpdateCourse(course);

                if (result)
                {
                    TempData["Message"] = "Course updated successfully!";
                }
                else
                {
                    TempData["Message"] = "Update failed!";
                }
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            bool result = CourseDB.DeleteCourse(id);
            if (result)
            {
                TempData["Message"] = "Course deleted successfully!";
            }
            else
            {
                TempData["Message"] = "Delete failed!";
            }
            return RedirectToAction("Index");
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
