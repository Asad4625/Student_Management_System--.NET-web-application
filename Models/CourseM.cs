using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMSProject.Models
{
    public class CourseM
    {
        public Course NewCourse { get; set; } = new Course();
        public List<Course> CourseList { get; set; } = new List<Course>();

        public Student NewStudent { get; set; } = new Student();
        public List<Student> StudentList { get; set; } = new List<Student>();
        public Enrollment NewEnroll { get; set; } = new Enrollment();
        public List<Enrollment> EnrollList { get; set; } = new List<Enrollment>();
        // Pagination properties
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }


        public Student Student { get; set; }
        public List<Course> CurrentSemesterCourses { get; set; }
        public List<Enrollment> AcademicHistory { get; set; }
        public int CompletedCourses { get; set; }
        public int EnrolledCourses { get; set; }
        public int FailedCourses { get; set; }
        public decimal CompletionRate { get; set; }



        public List<StudentSignup> PendingSignups { get; set; } = new List<StudentSignup>();

        public List<StudentSignup> PendingSignupsStd { get; set; } = new List<StudentSignup>();
        public List<Student> ApprovedSignupsStd { get; set; } = new List<Student>();
        
    }
}