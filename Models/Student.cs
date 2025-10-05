using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SMSProject.Models
{
    public class Student
    {
        [Key]
        [Required(ErrorMessage = "Student ID is required.")]
        [RegularExpression(@"^\d{4}-[A-Z]{4}-\d{4}$", ErrorMessage = "Student ID format must be like 2025-BIIT-4266")]
        public string StudentID { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "CNIC is required.")]
        [RegularExpression(@"^\d{5}-\d{7}-\d$", ErrorMessage = "CNIC format must be like 12345-1234567-1")]
        public string CNIC { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone is required.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Phone number must be 11 digits.")]
        public string Phone { get; set; }

        public byte[] Photo { get; set; }
    }

}
