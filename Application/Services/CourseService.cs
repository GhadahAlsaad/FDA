using Application.DTOs.Course;
using Application.Repositories.Interfaces;
using Application.Services.Interfaces;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace Application.Services
{
    public class CourseService : ICourseService
    {
        private readonly IHttpContextAccessor _http;
        private readonly IGenericRepository<Course> _courseRepo;
        private readonly IGenericRepository<Category> _categoryRepo;
        private readonly IGenericRepository<Enrollment> _enrollmentRepo;

        public CourseService(
            IHttpContextAccessor httpContextAccessor,
            IGenericRepository<Course> courseRepo,
            IGenericRepository<Category> categoryRepo,
            IGenericRepository<Enrollment> enrollmentRepo)
        {
            _http = httpContextAccessor;
            _courseRepo = courseRepo;
            _categoryRepo = categoryRepo;
            _enrollmentRepo = enrollmentRepo;
        }

        public async Task CreateCourseAsync(CourseDto input)
        {
            var role = _http.HttpContext.User.FindFirstValue(ClaimTypes.Role);
            if (role != FDAConst.ADMIN_ROLE)
                throw new UnauthorizedAccessException("Only admin");

            if (input.Price < 0)
                throw new Exception("Price must be >= 0");

            if (input.EndDate < input.StartDate)
                throw new Exception("EndDate must be >= StartDate");

            var catExists = await _categoryRepo.GetAll()
                .AnyAsync(c => c.Id == input.CategoryId);
            if (!catExists)
                throw new Exception("Category not found");

            await _courseRepo.Insert(new Course
            {
                Title = input.Title,
                Description = input.Description,
                Price = input.Price,
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                CategoryId = input.CategoryId
            });

            await _courseRepo.SaveChanges();
        }

        public async Task UpdateCourseAsync(int courseId, CourseDto input)
        {
            var role = _http.HttpContext.User.FindFirstValue(ClaimTypes.Role);
            if (role != FDAConst.ADMIN_ROLE)
                throw new UnauthorizedAccessException("Only admin");

            var course = await _courseRepo.GetAll()
                .FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null)
                throw new Exception("Course not found");

            if (input.Price < 0)
                throw new Exception("Price must be >= 0");

            if (input.EndDate < course.StartDate)
                throw new Exception("EndDate must be >= StartDate");

            var catExists = await _categoryRepo.GetAll()
                .AnyAsync(c => c.Id == input.CategoryId);
            if (!catExists)
                throw new Exception("Category not found");

            course.Title = input.Title;
            course.Description = input.Description;
            course.Price = input.Price;
            course.EndDate = input.EndDate;
            course.CategoryId = input.CategoryId;

            _courseRepo.Update(course);
            await _courseRepo.SaveChanges();
        }

        public async Task DeleteCourseAsync(int courseId)
        {
            var role = _http.HttpContext.User.FindFirstValue(ClaimTypes.Role);
            if (role != FDAConst.ADMIN_ROLE)
                throw new UnauthorizedAccessException("Only admin");

            var course = await _courseRepo.GetById(courseId);
            if (course == null)
                throw new Exception("Course not found");

            var hasEnrollments = await _enrollmentRepo.GetAll()
                .AnyAsync(e => e.CourseId == courseId);
            if (hasEnrollments)
                throw new Exception("Cannot delete course because students are enrolled");

            _courseRepo.Delete(course);
            await _courseRepo.SaveChanges();
        }

        public async Task<List<CourseDto>> GetCoursesAsync(string? name, int? categoryId)
        {
            var role = _http.HttpContext.User.FindFirstValue(ClaimTypes.Role);
            if (role != FDAConst.ADMIN_ROLE)
                throw new UnauthorizedAccessException("Only admin");

            var courses = _courseRepo.GetAll()
                .Include(x => x.Category).Where(x =>
                (!string.IsNullOrWhiteSpace(name)? x.Title.Trim().ToLower()
                .Contains(name.Trim().ToLower()): true)
                && (categoryId.HasValue? x.CategoryId == categoryId.Value: true));


            var list = await courses.ToListAsync();

            return list.Select(c => new CourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Price = c.Price,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                CategoryId = c.CategoryId
            }).ToList();
        }

        public async Task<CourseDto> GetCourseByIdAsync(int courseId)
        {
            var role = _http.HttpContext.User.FindFirstValue(ClaimTypes.Role);
            if (role != FDAConst.ADMIN_ROLE)
                throw new UnauthorizedAccessException("Only admin");

            var c = await _courseRepo.GetAll()
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == courseId);

            if (c == null)
                throw new Exception("Course not found");

            return new CourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Price = c.Price,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                CategoryId = c.CategoryId
            };
        }
    }
}
