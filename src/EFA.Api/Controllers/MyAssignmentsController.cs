using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EFA.Api.Common;
using EFA.Application.Assignments.GetMyAssignments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFA.Api.Controllers
{
    [ApiController]
    [Route("api/assignments")]
    [Authorize]
    public sealed class MyAssignmentsController : ControllerBase
    {
        private readonly GetMyAssignmentsHandler _getMyAssignmentsHandler;

        public MyAssignmentsController(GetMyAssignmentsHandler getMyAssignmentsHandler)
        {
            _getMyAssignmentsHandler = getMyAssignmentsHandler;
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyAssignments(
            [FromQuery] string? type,
            [FromQuery] string? period,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    "Invalid token. User id was not found in token."));
            }

            var result = await _getMyAssignmentsHandler.HandleAsync(
                new GetMyAssignmentsQuery
                {
                    UserId = userId,
                    Type = type,
                    Period = period
                },
                cancellationToken);

            if (result.IsNotFound)
            {
                return NotFound(ApiResponse<object>.Fail(
                    "Member profile was not found for the current user.",
                    result.Errors));
            }

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to retrieve assignments.",
                    result.Errors));
            }

            return Ok(ApiResponse<List<MyAssignmentResponse>>.Success(
                result.Data!,
                "My assignments retrieved successfully."));
        }
    }
}
