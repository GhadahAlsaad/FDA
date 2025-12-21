using Application.DTOs.Student;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface IStudentService
    {
        Task RegisterStudentAsync(RegisterStudentDto input);
        Task UpdateMyProfileAsync(UpdateMyProfileDto input);
        Task ChangeStudentPasswordAsync(ChangePasswordDto input);
        Task DeleteStudentAsync(int userId);
        Task<List<StudentDetailsDto>> GetStudentsAsync();
        Task<StudentDetailsDto> GetStudentByIdAsync(int userId);
    }
}

