using Ecommerce.core.DTos.Auth;
using Ecommerce.core.DTOs.Auth;
using Ecommerce.core.interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Ecommerce_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(
            IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [EnableRateLimiting("AuthLimiter")]
        public async Task<IActionResult> Register(
            RegisterRequest request)
        {
            var result =
                await _authService.RegisterAsync(request);

            if (result == null)
                return BadRequest("Email already exists");

            return Ok(result);
        }

        [HttpPost("login")]
        [EnableRateLimiting("AuthLimiter")]
        public async Task<IActionResult> Login(
            LoginRequest request)
        {
            var result =
                await _authService.LoginAsync(request);

            if (result == null)
                return Unauthorized();

            return Ok(result);
        }

        [HttpPost("refresh")]
        [EnableRateLimiting("AuthLimiter")]
        public async Task<IActionResult> Refresh(
            RefreshRequest request)
        {
            var result =
                await _authService.RefreshTokenAsync(request);

            if (result == null)
                return Unauthorized();

            return Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(
            LogoutRequest request)
        {
            var result =
                await _authService.LogoutAsync(request);

            if (!result)
                return BadRequest();

            return Ok();
        }
    }
}