using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMSProject.Models
{
    public class Enrollment
    {
       
            public string EnrollmentID { get; set; }
            public string StudentID { get; set; }
            public int CourseID { get; set; }
            public string Department { get; set; }
            public DateTime EnrollDate { get; set; }
            public int Semester { get; set; }
            public string CoursesName { get; set; }
            public string Status { get; set; }
            public string StudentName { get; set; }
            public string Gender { get; set; }
            public byte[] Photo { get; set; }

    }
}