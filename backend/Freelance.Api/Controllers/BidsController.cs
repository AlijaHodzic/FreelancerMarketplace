using Freelance.Application.DTOs.Bids;
using Freelance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Freelance.Api.Controllers;

[Authorize(Roles = "Freelancer")]
[ApiController]
[Route("api/[controller]")]
public class BidsController : ControllerBase
{
    private readonly IBidService _bidService;

    public BidsController(IBidService bidService)
    {
        _bidService = bidService;
    }

    // POST: api/bids
    [HttpPost]
    public async Task<ActionResult<BidResponse>> Create([FromBody] CreateBidRequest request)
    {
        var freelancerId = GetUserId();
        var result = await _bidService.CreateAsync(freelancerId, request);
        return Ok(result);
    }

    // GET: api/bids/project/{projectId}
    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<IEnumerable<BidResponse>>> GetByProject(Guid projectId)
    {
        var requesterId = GetUserId();
        var result = await _bidService.GetByProjectAsync(projectId, requesterId);
        return Ok(result);
    }

    // POST: api/bids/{bidId}/accept
    [HttpPost("{bidId}/accept")]
    public async Task<IActionResult> Accept(Guid bidId)
    {
        var ownerId = GetUserId();
        await _bidService.AcceptAsync(bidId, ownerId);
        return NoContent();
    }

    // POST: api/bids/{bidId}/reject
    [HttpPost("{bidId}/reject")]
    public async Task<IActionResult> Reject(Guid bidId)
    {
        var ownerId = GetUserId();
        await _bidService.RejectAsync(bidId, ownerId);
        return NoContent();
    }

    // ===================== HELPERS =====================
    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("User ID not found in token");

        return Guid.Parse(userId);
    }
}
