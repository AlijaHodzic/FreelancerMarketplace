using Freelance.Domain.Enums;

namespace Freelance.Application.DTOs.Admin;

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
