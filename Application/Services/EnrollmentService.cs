using Application.DTOs.Enrollment;
using Application.Repositories.Interfaces;
using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Application.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGenericRepository<Enrollment> _enrollmentRepo;
        private readonly IGenericRepository<Course> _courseRepo;

        public EnrollmentService(
            IHttpContextAccessor httpContextAccessor,
            IGenericRepository<Enrollment> enrollmentRepo,
            IGenericRepository<Course> courseRepo)
        {
            _httpContextAccessor = httpContextAccessor;
            _enrollmentRepo = enrollmentRepo;
            _courseRepo = courseRepo;
        }

        public async Task StudentEnrollmentAsync(EnrollmentDto input)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim))
                throw new UnauthorizedAccessException("Unauthorized");

            var userId = Convert.ToInt32(userIdClaim);

            var courseExists = await _courseRepo.GetAll().AnyAsync(c => c.Id == input.CourseId);
            if (!courseExists)
                throw new Exception("Course not found");

            var alreadyEnrolled = await _enrollmentRepo.GetAll()
                .AnyAsync(e => e.UserId == userId && e.CourseId == input.CourseId);

            if (alreadyEnrolled)
                throw new Exception("You are already enrolled in this course.");

            await _enrollmentRepo.Insert(new Enrollment
            {
                UserId = userId,
                CourseId = input.CourseId
            });

            await _enrollmentRepo.SaveChanges();
        }

        public async Task<List<EnrollmentDto>> GetMyEnrollmentsAsync()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim))
                throw new UnauthorizedAccessException("Unauthorized");

            var userId = Convert.ToInt32(userIdClaim);

            var list = await _enrollmentRepo.GetAll()
                .Include(e => e.Course).ThenInclude(c => c.Category)
                .Where(e => e.UserId == userId)
                .ToListAsync();

            return list.Select(e => new EnrollmentDto
            {
                Id = e.Id,
                CourseId = e.CourseId,
                UserId = e.UserId,
                EnrollmentDate = e.EnrollmentDate,
                CourseTitle = e.Course.Title,
                Price = e.Course.Price,
                StartDate = e.Course.StartDate,
                EndDate = e.Course.EndDate,
                CategoryId = e.Course.CategoryId,
                CategoryName = e.Course.Category?.Name
            }).ToList();
        }

        public async Task<List<EnrollmentDto>> GetStudentEnrollmentsAsync(int userId)
        {
            var list = await _enrollmentRepo.GetAll()
                .Include(e => e.Course).ThenInclude(c => c.Category)
                .Where(e => e.UserId == userId)
                .ToListAsync();

            return list.Select(e => new EnrollmentDto
            {
                Id = e.Id,
                CourseId = e.CourseId,
                UserId = e.UserId,
                EnrollmentDate = e.EnrollmentDate,
                CourseTitle = e.Course.Title,
                Price = e.Course.Price,
                StartDate = e.Course.StartDate,
                EndDate = e.Course.EndDate,
                CategoryId = e.Course.CategoryId,
                CategoryName = e.Course.Category?.Name
            }).ToList();
        }
    }
}
