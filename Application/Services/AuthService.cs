using Application.DTOs.Auth;
using Application.Repositories.Interfaces;
using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<RefreshToken> _refreshTokenRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(IConfiguration config,
                           IGenericRepository<User> userRepo,
                           IHttpContextAccessor httpContextAccessor,
                           IGenericRepository<RefreshToken> refreshTokenRepo)
        {
            _config = config;
            _userRepo = userRepo;
            _httpContextAccessor = httpContextAccessor;
            _refreshTokenRepo = refreshTokenRepo;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto input)
        {
            var user = await _userRepo.GetAll()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email.Trim().ToLower() == input.Email.Trim().ToLower());

            if (user == null)
            {
                return null;
            }
            var passwordHasher = new PasswordHasher<User>();
            var passwordResult = passwordHasher.VerifyHashedPassword(user, user.Password, input.Password);

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                return null;
            }
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            await _refreshTokenRepo.Insert(new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return new LoginResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Role = new LoginRolesResponseDto
                {
                    Id = user.Role.Id,
                    Name = user.Role.Name,
                    Code = user.Role.Code
                }
            };
        }


        public string GenerateAccessToken(User user)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("PhoneNumber", user.PhoneNumber ?? ""),
                new Claim(ClaimTypes.Role, user.Role.Name),
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                Issuer = jwtSection["Issuer"],
                Audience = jwtSection["Audience"],
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }



        public string GenerateRefreshToken()
        {
            var random = new byte[64];
            RandomNumberGenerator.Fill(random);
            return Convert.ToBase64String(random);
        }

        public async Task<string> RefreshToken(string refreshToken)
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Convert.ToInt32(userIdClaim);

            var storedToken = _refreshTokenRepo.GetAll()
                .FirstOrDefault(rt => rt.UserId == userId && rt.Token == refreshToken && rt.Expires > DateTime.UtcNow);

            if (storedToken == null)
            {
                throw new SecurityTokenException("Invalid refresh token.");
            }

            var user = await _userRepo.GetById(storedToken.UserId);
            return GenerateAccessToken(user);
        }

        public async Task ResetPassword(ResetPasswordDto input)
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Convert.ToInt32(userIdClaim);
            var user = await _userRepo.GetById(userId);

            var passwordHasher = new PasswordHasher<User>();
            var passowrdResult = passwordHasher.VerifyHashedPassword(user, user.Password, input.OldPassword);

            if (passowrdResult == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException("Old password is incorrect.");
            }

            user.Password = passwordHasher.HashPassword(user, input.NewPassword);
            _userRepo.Update(user);
            await _userRepo.SaveChanges();
        }


    }
}