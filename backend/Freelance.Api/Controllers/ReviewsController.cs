using System.Security.Claims;
using Freelance.Application.DTOs.Reviews;
using Freelance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Freelance.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    [Authorize(Roles = "Client,Admin")]
    public async Task<ActionResult<ReviewDto>> Create([FromBody] CreateReviewRequest request)
    {
        try
        {
            return Ok(await _reviewService.CreateAsync(GetUserId(), request));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("freelancer/{freelancerId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetForFreelancer(Guid freelancerId)
    {
        return Ok(await _reviewService.GetForFreelancerAsync(freelancerId));
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
