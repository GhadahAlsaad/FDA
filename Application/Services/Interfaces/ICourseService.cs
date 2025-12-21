using Application.DTOs.Course;

namespace Application.Services.Interfaces
{
    public interface ICourseService
    {
        Task CreateCourseAsync(CourseDto input);
        Task UpdateCourseAsync(int courseId, CourseDto input);
        Task DeleteCourseAsync(int courseId); 
        Task<List<CourseDto>> GetCoursesAsync(string? name, int? categoryId);
        Task<CourseDto> GetCourseByIdAsync(int courseId);
    }
}
