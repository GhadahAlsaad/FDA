using Application.DTOs.Course;
using Application.Services.Interfaces;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Future_Dev_Academy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = FDAConst.ADMIN_ROLE)]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpPost("CreateCourse")]
        public async Task<IActionResult> CreateCourse([FromBody] CourseDto input)
        {
            await _courseService.CreateCourseAsync(input);
            return Ok();
        }

        [HttpPut("UpdateCourse/{courseId}")]
        public async Task<IActionResult> UpdateCourse(int courseId, [FromBody] CourseDto input)
        {
            await _courseService.UpdateCourseAsync(courseId, input);
            return Ok();
        }

        [HttpDelete("DeleteCourse/{courseId}")]
        public async Task<IActionResult> DeleteCourse(int courseId)
        {
            await _courseService.DeleteCourseAsync(courseId);
            return Ok();
        }

        [HttpGet("GetCourses")]
        public async Task<IActionResult> GetCourses([FromQuery] string? name, [FromQuery] int? categoryId)
        {
            var courses = await _courseService.GetCoursesAsync(name, categoryId);
            return Ok(courses);
        }

        [HttpGet("GetCourseById/{courseId}")]
        public async Task<IActionResult> GetCourseById(int courseId)
        {
            var course = await _courseService.GetCourseByIdAsync(courseId);
            return Ok(course);
        }
    }
}
