using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Student
{
    public class RegisterStudentDto
    {
        public string Name { get; set; }
        [MaxLength(200)]
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string Password { get; set; }

        public DateTime BirthDate { get; set; }

        public string UniversityName { get; set; }
    }
}
