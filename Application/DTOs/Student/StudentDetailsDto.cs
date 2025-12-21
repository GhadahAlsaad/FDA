using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Student
{
    public class StudentDetailsDto
    {
        public int UserId { get; set; }
        public int StudentId { get; set; }
        public DateTime BirthDate { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string UniversityName { get; set; }
        public string RoleName { get; set; }
    }
}
