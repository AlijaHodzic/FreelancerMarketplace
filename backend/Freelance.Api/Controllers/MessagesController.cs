using Freelance.Application.DTOs.Messages;
using Freelance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Freelance.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessagingService _messagingService;

    public MessagesController(IMessagingService messagingService)
    {
        _messagingService = messagingService;
    }

    [HttpGet("conversations")]
    public async Task<ActionResult<IEnumerable<ConversationSummaryDto>>> GetConversations()
    {
        try
        {
            var result = await _messagingService.GetConversationsAsync(GetUserId());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("conversations/{conversationId:guid}")]
    public async Task<ActionResult<ConversationThreadDto>> GetConversation(Guid conversationId)
    {
        try
        {
            var result = await _messagingService.GetConversationAsync(GetUserId(), conversationId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("conversations")]
    public async Task<ActionResult<ConversationThreadDto>> StartConversation([FromBody] StartConversationRequest request)
    {
        try
        {
            var result = await _messagingService.StartConversationAsync(GetUserId(), request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("conversations/{conversationId:guid}/messages")]
    public async Task<ActionResult<MessageDto>> SendMessage(Guid conversationId, [FromBody] SendMessageRequest request)
    {
        try
        {
            var result = await _messagingService.SendMessageAsync(GetUserId(), conversationId, request);
            return Ok(result);
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
