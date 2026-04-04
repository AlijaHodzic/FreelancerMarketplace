using BCrypt.Net;
using Freelance.Application.DTOs.Auth;
using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Freelance.Domain.Enums;
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
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var emailExists = await _db.Users.AnyAsync(u => u.Email == normalizedEmail);
            if (emailExists)
                throw new Exception("Email already exists");

            if (request.Role is not UserRole.Client and not UserRole.Freelancer)
                throw new Exception("Only client and freelancer accounts can be registered");

            var fullName = request.FullName.Trim();
            var slug = await GenerateUniqueSlugAsync(fullName);

            var user = new User
            {
                Email = normalizedEmail,
                FullName = fullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
                Slug = slug,
                Headline = request.Role == UserRole.Freelancer ? "New Freelancer" : "Client",
                ShortDescription = request.Role == UserRole.Freelancer
                    ? "New freelancer profile. Complete your profile to start attracting better projects."
                    : "New client account ready to post projects and hire freelancers.",
                About = request.Role == UserRole.Freelancer
                    ? "Tell clients about your experience, tools, and the kind of work you do best."
                    : "Client account ready to publish projects and manage proposals.",
            };

            _db.Users.Add(user);
            var refreshToken = CreateRefreshToken(user.Id);
            _db.RefreshTokens.Add(refreshToken);

            await _db.SaveChangesAsync();

            return CreateAuthResponse(user, refreshToken.Token);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
            if (user == null)
                throw new Exception("Invalid credentials");

            var validPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!validPassword)
                throw new Exception("Invalid credentials");

            var refreshToken = CreateRefreshToken(user.Id);
            _db.RefreshTokens.Add(refreshToken);
            await _db.SaveChangesAsync();

            return CreateAuthResponse(user, refreshToken.Token);
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

            return CreateAuthResponse(token.User, newRefreshToken.Token);
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

        private RefreshToken CreateRefreshToken(Guid userId)
        {
            return new RefreshToken
            {
                UserId = userId,
                Token = RefreshTokenGenerator.GenerateToken(),
                ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
            };
        }

        private AuthResponse CreateAuthResponse(User user, string refreshToken)
        {
            return new AuthResponse
            {
                AccessToken = _jwt.GenerateAccessToken(user),
                RefreshToken = refreshToken,
                User = new AuthUserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role
                }
            };
        }

        private async Task<string> GenerateUniqueSlugAsync(string fullName)
        {
            var baseSlug = string.Join(
                "-",
                fullName
                    .Trim()
                    .ToLowerInvariant()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries));

            if (string.IsNullOrWhiteSpace(baseSlug))
            {
                baseSlug = "user";
            }

            var slug = baseSlug;
            var suffix = 2;

            while (await _db.Users.AnyAsync(user => user.Slug == slug))
            {
                slug = $"{baseSlug}-{suffix}";
                suffix++;
            }

            return slug;
        }
    }
}
