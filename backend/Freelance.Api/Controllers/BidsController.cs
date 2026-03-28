using Freelance.Application.DTOs.Bids;
using Freelance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Freelance.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BidsController : ControllerBase
{
    private readonly IBidService _bidService;

    public BidsController(IBidService bidService)
    {
        _bidService = bidService;
    }

    [HttpPost]
    [Authorize(Roles = "Freelancer,Admin")]
    public async Task<ActionResult<BidResponse>> Create([FromBody] CreateBidRequest request)
    {
        try
        {
            var freelancerId = GetUserId();
            var result = await _bidService.CreateAsync(freelancerId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("project/{projectId}")]
    [Authorize(Roles = "Client,Admin")]
    public async Task<ActionResult<IEnumerable<BidResponse>>> GetByProject(Guid projectId)
    {
        try
        {
            var requesterId = GetUserId();
            var result = await _bidService.GetByProjectAsync(projectId, requesterId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("mine")]
    [Authorize(Roles = "Freelancer,Admin")]
    public async Task<ActionResult<IEnumerable<BidResponse>>> GetMine()
    {
        try
        {
            var freelancerId = GetUserId();
            var result = await _bidService.GetMineAsync(freelancerId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{bidId}/accept")]
    [Authorize(Roles = "Client,Admin")]
    public async Task<IActionResult> Accept(Guid bidId)
    {
        try
        {
            var ownerId = GetUserId();
            await _bidService.AcceptAsync(bidId, ownerId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{bidId}/reject")]
    [Authorize(Roles = "Client,Admin")]
    public async Task<IActionResult> Reject(Guid bidId)
    {
        try
        {
            var ownerId = GetUserId();
            await _bidService.RejectAsync(bidId, ownerId);
            return NoContent();
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
            throw new UnauthorizedAccessException("User ID not found in token");

        return Guid.Parse(userId);
    }
}
