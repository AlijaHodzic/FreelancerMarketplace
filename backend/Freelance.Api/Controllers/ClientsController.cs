using System.Security.Claims;
using Freelance.Application.DTOs.Clients;
using Freelance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Freelance.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client,Admin")]
public class ClientsController : ControllerBase
{
    private readonly IClientProfileService _clientProfileService;

    public ClientsController(IClientProfileService clientProfileService)
    {
        _clientProfileService = clientProfileService;
    }

    [HttpGet("me/profile")]
    public async Task<ActionResult<ClientProfileDto>> GetMine()
    {
        try
        {
            return Ok(await _clientProfileService.GetMineAsync(GetUserId()));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("me/profile")]
    public async Task<ActionResult<ClientProfileDto>> Update([FromBody] UpdateClientProfileRequest request)
    {
        try
        {
            return Ok(await _clientProfileService.UpdateAsync(GetUserId(), request));
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
