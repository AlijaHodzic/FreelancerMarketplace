using Freelance.Application.DTOs.Admin;
using Freelance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Freelance.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminDashboardService _adminDashboardService;

    public AdminController(IAdminDashboardService adminDashboardService)
    {
        _adminDashboardService = adminDashboardService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<AdminSummaryDto>> GetSummary()
    {
        return Ok(await _adminDashboardService.GetSummaryAsync());
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserDto>>> GetUsers()
    {
        return Ok(await _adminDashboardService.GetUsersAsync());
    }

    [HttpGet("projects")]
    public async Task<ActionResult<List<AdminProjectDto>>> GetProjects()
    {
        return Ok(await _adminDashboardService.GetProjectsAsync());
    }

    [HttpGet("activity")]
    public async Task<ActionResult<List<AdminActivityDto>>> GetActivity()
    {
        return Ok(await _adminDashboardService.GetActivityAsync());
    }
}
