using Freelance.Application.DTOs.Auth;
using Freelance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Freelance.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

       
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }

       
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }

        
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshAsync(request);
            return Ok(result);
        }

       
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
        {
            await _authService.LogoutAsync(request.RefreshToken);
            return NoContent();
        }

      
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            return Ok(new
            {
                User = User.Identity?.Name,
                Claims = User.Claims.Select(c => new { c.Type, c.Value })
            });
        }
    }
}
