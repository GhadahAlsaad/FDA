using Application.DTOs.Auth;
using Domain.Entities;

namespace Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto input);

        string GenerateAccessToken(User user);

        string GenerateRefreshToken();

        Task ResetPassword(ResetPasswordDto input);

        Task<string> RefreshToken(string refreshToken);
    }
}