using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EFA.Api.Common;
using EFA.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace EFA.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Username is required.",
                    new List<string> { "UserName cannot be empty." }));
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Password is required.",
                    new List<string> { "Password cannot be empty." }));
            }

            var user = await _userManager.FindByNameAsync(request.UserName);

            if (user is null)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    "Invalid username or password."));
            }

            if (!user.IsActive)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(
                        "This account is deactivated. Please contact system administrator."));
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return StatusCode(StatusCodes.Status423Locked,
                    ApiResponse<object>.Fail(
                        "This account is locked due to multiple failed login attempts. Please try again later."));
            }

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                return StatusCode(StatusCodes.Status423Locked,
                    ApiResponse<object>.Fail(
                        "This account has been locked due to multiple failed login attempts."));
            }

            if (!result.Succeeded)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    "Invalid username or password."));
            }

            var roles = await _userManager.GetRolesAsync(user);

            var expiresAt = DateTime.Now.AddHours(2);

            var token = GenerateJwtToken(user, roles.ToList(), expiresAt);

            var response = new LoginResponse
            {
                AccessToken = token,
                ExpiresAt = expiresAt,
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                FullName = user.FullName,
                Roles = roles.ToList()
            };

            return Ok(ApiResponse<LoginResponse>.Success(
                response,
                "Login completed successfully."));
        }
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    "Invalid token. User id was not found in token."));
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    "User was not found."));
            }

            if (!user.IsActive)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(
                        "This account is deactivated."));
            }

            var roles = await _userManager.GetRolesAsync(user);

            var response = new
            {
                userId = user.Id,
                userName = user.UserName,
                email = user.Email,
                fullName = user.FullName,
                roles = roles.ToList()
            };

            return Ok(ApiResponse<object>.Success(
                response,
                "Current user loaded successfully."));
        }
        private string GenerateJwtToken(
            ApplicationUser user,
            List<string> roles,
            DateTime expiresAt)
        {
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim("fullName", user.FullName)
        };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var secretKey = _configuration["Jwt:SecretKey"];

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is missing.");
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public sealed class LoginRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public sealed class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}
