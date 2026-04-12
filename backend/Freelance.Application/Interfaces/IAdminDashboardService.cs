using Freelance.Application.DTOs.Admin;

namespace Freelance.Application.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminSummaryDto> GetSummaryAsync();
}
