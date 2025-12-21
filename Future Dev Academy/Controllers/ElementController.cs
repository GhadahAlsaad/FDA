using Application.DTOs.Enrollment;
using Application.Services.Interfaces;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Future_Dev_Academy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EnrollmentController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;

        public EnrollmentController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        [Authorize(Roles = FDAConst.STUDENT_ROLE)]
        [HttpPost("StudentEnrollment")]
        public async Task<IActionResult> StudentEnrollment([FromBody] EnrollmentDto input)
        {
            try
            {
                await _enrollmentService.StudentEnrollmentAsync(input);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = FDAConst.STUDENT_ROLE)]
        [HttpGet("GetMyEnrollment")]
        public async Task<IActionResult> GetMyEnrollment()
        {
            var result = await _enrollmentService.GetMyEnrollmentsAsync();
            return Ok(result);
        }

        [Authorize(Roles = FDAConst.ADMIN_ROLE)]
        [HttpGet("GetStudentEnrollments/{userId}")]
        public async Task<IActionResult> GetStudentEnrollments(int userId)
        {
            var result = await _enrollmentService.GetStudentEnrollmentsAsync(userId);
            return Ok(result);
        }
    }
}
