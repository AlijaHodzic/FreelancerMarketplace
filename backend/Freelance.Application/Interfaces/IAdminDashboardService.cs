using Freelance.Application.DTOs.Admin;

namespace Freelance.Application.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminSummaryDto> GetSummaryAsync();
    Task<List<AdminUserDto>> GetUsersAsync();
    Task<List<AdminProjectDto>> GetProjectsAsync();
    Task<List<AdminActivityDto>> GetActivityAsync();
}
