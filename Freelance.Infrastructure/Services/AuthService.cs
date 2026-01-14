using BCrypt.Net;
using Freelance.Application.DTOs.Auth;
using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Freelance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Freelance.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly JwtTokenService _jwt;

        public AuthService(AppDbContext db, JwtTokenService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var emailExists = await _db.Users.AnyAsync(u => u.Email == request.Email);
            if (emailExists)
                throw new Exception("Email already exists");

            var user = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = _jwt.GenerateAccessToken(user),
                RefreshToken = "TEMP_REFRESH_TOKEN"
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                throw new Exception("Invalid credentials");

            var validPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!validPassword)
                throw new Exception("Invalid credentials");

            return new AuthResponse
            {
                AccessToken = _jwt.GenerateAccessToken(user),
                RefreshToken = "TEMP_REFRESH_TOKEN"
            };
        }
        public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request)
        {
            var token = await _db.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.Token == request.RefreshToken &&
                    x.RevokedAtUtc == null &&
                    x.ExpiresAtUtc > DateTime.UtcNow);

            if (token == null)
                throw new Exception("Invalid refresh token");

            token.RevokedAtUtc = DateTime.UtcNow;

            var newRefreshToken = CreateRefreshToken(token.UserId);
            _db.RefreshTokens.Add(newRefreshToken);

            await _db.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = _jwt.GenerateAccessToken(token.User),
                RefreshToken = newRefreshToken.Token
            };
        }

        private RefreshToken CreateRefreshToken(Guid userId)
        {
            return new RefreshToken
            {
                UserId = userId,
                Token = RefreshTokenGenerator.GenerateToken(),
                ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
            };
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var token = await _db.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshToken && x.RevokedAtUtc == null);

            if (token != null)
            {
                token.RevokedAtUtc = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }


    }
}
