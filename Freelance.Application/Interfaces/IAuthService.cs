using Freelance.Application.DTOs.Auth;

namespace Freelance.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RefreshAsync(RefreshTokenRequest request);
        Task LogoutAsync(string refreshToken);
    }
}
