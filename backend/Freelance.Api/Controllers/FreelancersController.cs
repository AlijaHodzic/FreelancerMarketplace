using Freelance.Application.DTOs.Freelancers;
using Freelance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Freelance.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FreelancersController : ControllerBase
{
    private readonly IFreelancerProfileService _freelancerProfileService;

    public FreelancersController(IFreelancerProfileService freelancerProfileService)
    {
        _freelancerProfileService = freelancerProfileService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FreelancerSummaryDto>>> GetAll()
    {
        return Ok(await _freelancerProfileService.GetAllAsync());
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<FreelancerProfileDto>> GetBySlug(string slug)
    {
        try
        {
            return Ok(await _freelancerProfileService.GetBySlugAsync(slug));
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("me/profile")]
    [Authorize(Roles = "Freelancer,Admin")]
    public async Task<ActionResult<FreelancerProfileDto>> GetMine()
    {
        try
        {
            return Ok(await _freelancerProfileService.GetMineAsync(GetUserId()));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("me/profile")]
    [Authorize(Roles = "Freelancer,Admin")]
    public async Task<ActionResult<FreelancerProfileDto>> Update([FromBody] UpdateFreelancerProfileRequest request)
    {
        try
        {
            return Ok(await _freelancerProfileService.UpdateAsync(GetUserId(), request));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }

        return Guid.Parse(userId);
    }
}
