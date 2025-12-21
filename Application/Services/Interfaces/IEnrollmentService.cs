using Application.DTOs.Enrollment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Enrollment;

namespace Application.Services.Interfaces
{
    public interface IEnrollmentService
    {
        Task StudentEnrollmentAsync(EnrollmentDto input);
        Task<List<EnrollmentDto>> GetMyEnrollmentsAsync();
        Task<List<EnrollmentDto>> GetStudentEnrollmentsAsync(int userId);
    }
}
