using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SMSProject.Models
{
    public class StudentSignup
    {
        public int Id { get; set; }

        [Required]
        [RegularExpression(@"^\d{4}-BIIT-\d{4}$", ErrorMessage = "StudentID must be in the format: 2025-BIIT-4261")]
        public string StudentID { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
        public string Status { get; set; }

    }

   
}