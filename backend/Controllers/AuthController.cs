using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SculptureMonitor.Models;
using SculptureMonitor.Services;

namespace SculptureMonitor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var token = await _authService.LoginAsync(request.Username, request.Password);
                return Ok(new 
                { 
                    token, 
                    user = new 
                    { 
                        username = request.Username,
                        role = "admin"
                    } 
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "用户名或密码错误" });
            }
        }

        [HttpPost("register")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<User>> Register([FromBody] RegisterRequest request)
        {
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Role = request.Role,
                Phone = request.Phone
            };

            var created = await _authService.RegisterAsync(user, request.Password);
            return CreatedAtAction(nameof(Login), new { id = created.Id }, created);
        }

        [HttpGet("verify")]
        [Authorize]
        public IActionResult VerifyToken()
        {
            var claims = User.Claims.ToDictionary(c => c.Type, c => c.Value);
            return Ok(new { valid = true, claims });
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var isValid = await _authService.ValidateUserAsync(username, request.OldPassword);
            if (!isValid) return BadRequest(new { message = "原密码错误" });

            var user = new User { Username = username };
            await _authService.RegisterAsync(user, request.NewPassword);
            return Ok(new { success = true });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Phone { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
