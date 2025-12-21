using Application.DTOs.Student;
using Application.Repositories.Interfaces;
using Application.Services.Interfaces;
using Domain;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Application.Services
{
    public class StudentService : IStudentService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<Student> _studentRepo;
        private readonly IGenericRepository<Role> _roleRepo;
        private readonly IGenericRepository<Enrollment> _enrollmentRepo;
        private readonly IGenericRepository<RefreshToken> _refreshTokenRepo;

        public StudentService(IHttpContextAccessor httpContextAccessor,IGenericRepository<User> userRepo,IGenericRepository<Student> studentRepo,IGenericRepository<Role> roleRepo,IGenericRepository<Enrollment> enrollmentRepo,IGenericRepository<RefreshToken> refreshTokenRepo)
        {
            _httpContextAccessor = httpContextAccessor;
            _userRepo = userRepo;
            _studentRepo = studentRepo;
            _roleRepo = roleRepo;
            _enrollmentRepo = enrollmentRepo;
            _refreshTokenRepo = refreshTokenRepo;
        }

        public async Task RegisterStudentAsync(RegisterStudentDto input)
        {
            string passwordPattern = @"" + "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}$";
            if (!Regex.IsMatch(input.Password, passwordPattern)) 
                throw new Exception("Password is weak");

            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(input.Email, emailPattern)) 
                throw new Exception("Email is not valid");

            string mobilePattern = @"^(?:\+?962|00962)?0?7[7-9]\d{7}$";
            if (!Regex.IsMatch(input.PhoneNumber, mobilePattern)) 
                throw new Exception("Phone number is not valid");

            var studentRoleId = (await _roleRepo.GetAll()
                .FirstOrDefaultAsync(r => r.Code == SystemRole.Student))?.Id;

            var userObj = new User();
            userObj.Name = input.Name;
            userObj.Email = input.Email;
            userObj.PhoneNumber = input.PhoneNumber;
            userObj.RoleId = studentRoleId.Value;

            var passwordHasher = new PasswordHasher<User>();
            userObj.Password = passwordHasher.HashPassword(userObj, input.Password);

            await _userRepo.Insert(userObj);
            await _userRepo.SaveChanges();

            await _studentRepo.Insert(new Student
            {
                UserId = userObj.Id,
                BirthDate = input.BirthDate,
                UniversityName = input.UniversityName
            });

            await _studentRepo.SaveChanges();
        }

        public async Task UpdateMyProfileAsync(UpdateMyProfileDto input)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim)) 
                throw new UnauthorizedAccessException("Unauthorized");
            var userId = Convert.ToInt32(userIdClaim);

            var student = await _studentRepo.GetAll().Include(s => s.User).FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null)
                throw new Exception("Student not found");

            if (!string.IsNullOrWhiteSpace(input.PhoneNumber))
            {
                string mobilePattern = @"^(?:\+?962|00962)?0?7[7-9]\d{7}$";
                if (!Regex.IsMatch(input.PhoneNumber, mobilePattern)) 
                    throw new Exception("Phone number is not valid");

                var exists = await _userRepo.GetAll().AnyAsync(u => u.PhoneNumber == input.PhoneNumber && u.Id != userId);
                if (exists) 
                    throw new Exception("Phone number already exists");

                student.User.PhoneNumber = input.PhoneNumber;
            }

            student.User.Name = input.Name;
            student.User.Email = input.Email;
            student.User.PhoneNumber = input.PhoneNumber;
            student.UniversityName = input.UniversityName;
            student.BirthDate = input.BirthDate;

            _userRepo.Update(student.User);
            _studentRepo.Update(student);

            await _userRepo.SaveChanges();
            await _studentRepo.SaveChanges();
        }

        public async Task ChangeStudentPasswordAsync(ChangePasswordDto input)
        {
            var roleName = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
            if (roleName != FDAConst.ADMIN_ROLE) 
                throw new UnauthorizedAccessException("Only admin");

            var user = await _userRepo.GetAll().Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == input.UserId);
            if (user == null) 
                throw new Exception("User not found");
            if (user.Role == null || user.Role.Code != SystemRole.Student) 
                throw new Exception("Target is not student");

            var passwordHasher = new PasswordHasher<User>();
            user.Password = passwordHasher.HashPassword(user, input.NewPassword);

            _userRepo.Update(user);
            await _userRepo.SaveChanges();
        }

        public async Task DeleteStudentAsync(int userId)
        {
            var roleName = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
            if (roleName != FDAConst.ADMIN_ROLE) 
                throw new UnauthorizedAccessException("Only admin");

            var user = await _userRepo.GetById(userId);
            if (user == null) 
                throw new Exception("User not found");

            var student = await _studentRepo.GetAll().FirstOrDefaultAsync(x => x.UserId == user.Id);

            await _studentRepo.Delete(student);
            await _userRepo.Delete(user);
            await _userRepo.SaveChanges();
        }

        public async Task<List<StudentDetailsDto>> GetStudentsAsync()
        {
            var roleName = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
            if (roleName != FDAConst.ADMIN_ROLE) 
                throw new UnauthorizedAccessException("Only admin");

            var students = await _studentRepo.GetAll()
                .Include(s => s.User)
                .ToListAsync();

            return students.Select(s => new StudentDetailsDto
            {
                UserId = s.UserId,
                StudentId = s.Id,
                BirthDate = s.BirthDate,
                Name = s.User.Name,
                Email = s.User.Email,
                PhoneNumber = s.User.PhoneNumber,
                UniversityName = s.UniversityName
            }).ToList();
        }

        public async Task<StudentDetailsDto> GetStudentByIdAsync(int userId)
        {
            var roleName = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
            if (roleName != FDAConst.ADMIN_ROLE) 
                throw new UnauthorizedAccessException("Only admin");

            var student = await _studentRepo.GetAll()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null) 
                throw new Exception("Student not found");

            return new StudentDetailsDto
            {
                UserId = student.UserId,
                StudentId = student.Id,
                BirthDate = student.BirthDate,
                Name = student.User.Name,
                Email = student.User.Email,
                PhoneNumber = student.User.PhoneNumber,
                UniversityName = student.UniversityName
            };
        }
    }
}
