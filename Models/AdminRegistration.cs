using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SMSProject.Models
{
    public class AdminRegistration
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string CNIC { get; set; }

        [Required]

        [RegularExpression(@"^\d{11}$", ErrorMessage = "Invalid phone number. e.g: 03465")]
        public string Phone { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; }

        public byte[] Photo { get; set; }
        [NotMapped]
        public HttpPostedFileBase PhotoFile { get; set; }

    }
    public class AdminLoginForm
    {
        [Required]
        public string CNIC { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; }
    }

}