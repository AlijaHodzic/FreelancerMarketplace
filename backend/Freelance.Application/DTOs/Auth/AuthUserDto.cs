using Freelance.Domain.Enums;

namespace Freelance.Application.DTOs.Auth
{
    public class AuthUserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }
}
