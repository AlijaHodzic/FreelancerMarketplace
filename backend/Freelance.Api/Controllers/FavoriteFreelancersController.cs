using System.Security.Claims;
using Freelance.Application.DTOs.Freelancers;
using Freelance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Freelance.Api.Controllers;

[ApiController]
[Route("api/favorites/freelancers")]
[Authorize(Roles = "Client,Admin")]
public class FavoriteFreelancersController : ControllerBase
{
    private readonly IFavoriteFreelancerService _favoriteFreelancerService;

    public FavoriteFreelancersController(IFavoriteFreelancerService favoriteFreelancerService)
    {
        _favoriteFreelancerService = favoriteFreelancerService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FreelancerSummaryDto>>> GetMine()
    {
        return Ok(await _favoriteFreelancerService.GetMineAsync(GetUserId()));
    }

    [HttpPost("{freelancerId:guid}")]
    public async Task<IActionResult> Add(Guid freelancerId)
    {
        try
        {
            await _favoriteFreelancerService.AddAsync(GetUserId(), freelancerId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{freelancerId:guid}")]
    public async Task<IActionResult> Remove(Guid freelancerId)
    {
        await _favoriteFreelancerService.RemoveAsync(GetUserId(), freelancerId);
        return NoContent();
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
