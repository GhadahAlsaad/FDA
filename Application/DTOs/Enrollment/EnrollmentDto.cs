using System;

namespace Application.DTOs.Enrollment
{
    public class EnrollmentDto
    {
        public int CourseId { get; set; }
        public int? UserId { get; set; }
        public int? Id { get; set; }
        public DateTime? EnrollmentDate { get; set; }
        public string? CourseTitle { get; set; }
        public decimal? Price { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
