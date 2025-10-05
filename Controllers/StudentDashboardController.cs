using SMSProject.Database;
using SMSProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMSProject.Controllers
{
    public class StudentDashboardController : Controller
    {
        private SMSdatabase db = new SMSdatabase();

        public ActionResult Index()
        {
            ViewBag.AdminCNIC = Session["AdminCNIC"];
            string studentId = Session["AdminCNIC"]?.ToString();
            if (string.IsNullOrEmpty(studentId))
            {
                TempData["Error"] = "Please login to access the dashboard";
                return RedirectToAction("StudentLogin", "StudentSignup");
            }

            var student = db.GetStudentById(studentId);
            if (student.Photo != null && student.Photo.Length > 0)
            {
                string base64Image = Convert.ToBase64String(student.Photo);
                ViewBag.StudentPhoto = $"data:image/jpeg;base64,{base64Image}";
            }
            var enrollments = db.GetAllEnrollments().Where(e => e.StudentID == studentId).ToList();
            var courses = db.GetAllCourses();

            // Get current semester courses
            var currentSemesterCourses = enrollments
                .Where(e => e.Status == "Enrolled")
                .Join(courses,
                    e => e.CourseID,
                    c => c.Id,
                    (e, c) => c)
                .ToList();

            var viewModel = new CourseM
            {
                Student = student,
                CurrentSemesterCourses = currentSemesterCourses,
                AcademicHistory = enrollments,
                CompletedCourses = enrollments.Count(e => e.Status == "Completed"),
                EnrolledCourses = enrollments.Count(e => e.Status == "Enrolled"),
                FailedCourses = enrollments.Count(e => e.Status == "Failed"),
                CompletionRate = CalculateCompletionRate(enrollments)
            };

            return View(viewModel);
        }

        private decimal CalculateCompletionRate(List<Enrollment> enrollments)
        {
            if (!enrollments.Any()) return 0;

            int completed = enrollments.Count(e => e.Status == "Completed");
            int total = enrollments.Count;

            return Math.Round(((decimal)completed / total) * 100, 1);
        }
    }
}