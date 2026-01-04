using Application.DTOs.Student;
using Application.Services.Interfaces;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace Future_Dev_Academy.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpPut("UpdateMyProfile")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateMyProfileDto input)
        {
            await _studentService.UpdateMyProfileAsync(input);
            return Ok();
        }

        [Authorize(Roles = FDAConst.ADMIN_ROLE)]
        [HttpPost("GetStudents")]
        public async Task<IActionResult> GetStudents(string? name, string? universityName)
        {
            var result = await _studentService.GetStudentsAsync(name, universityName);
            return Ok(result);
        }

        [Authorize(Roles = FDAConst.ADMIN_ROLE)]
        [HttpGet("GetStudentById/{userId}")]
        public async Task<IActionResult> GetStudentById(int userId)
        {
            var result = await _studentService.GetStudentByIdAsync(userId);
            return Ok(result);
        }

        [Authorize(Roles = FDAConst.ADMIN_ROLE)]
        [HttpPost("ChangeStudentPassword")]
        public async Task<IActionResult> ChangeStudentPassword([FromBody] ChangePasswordDto input)
        {
            await _studentService.ChangeStudentPasswordAsync(input);
            return Ok();
        }

        [Authorize(Roles = FDAConst.ADMIN_ROLE)]
        [HttpDelete("DeleteStudent/{userId}")]
        public async Task<IActionResult> DeleteStudent(int userId)
        {
            await _studentService.DeleteStudentAsync(userId);
            return Ok();
        }
    }
}

