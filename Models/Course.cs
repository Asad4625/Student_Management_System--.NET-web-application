using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SMSProject.Models
{
    public class Course
    {
        public int Id { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Course code can only contain letters and numbers.")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "Course code must be 2 to 10 characters.")]
        public string CourseCode { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Course name must only contain alphabets and spaces.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Course name must be 3 to 50 characters.")]
        public string CourseName { get; set; }

        [Required]
        public string Department { get; set; }
        [Required]
        public int Credits { get; set; }
        [Required]
        public string Instructor { get; set; }
        [Required]
        public int Semester { get; set; }
        [Required]
        public int Capacity { get; set; }
  
        [Required]
        public string Description { get; set; }

        
    } 
  }


   

