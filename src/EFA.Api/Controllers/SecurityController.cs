using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EFA.Api.Common;
using EFA.Application.Security.GetMatchAccessLog;
using EFA.Application.Security.GetSecurityMatches;
using EFA.Application.Security.VerifyMemberAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFA.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SecurityOfficer,RefereeCommittee")]
    public sealed class SecurityController : ControllerBase
    {
        private readonly GetSecurityMatchesHandler _getSecurityMatchesHandler;
        private readonly VerifyMemberAccessHandler _verifyMemberAccessHandler;
        private readonly GetMatchAccessLogHandler _getMatchAccessLogHandler;

        public SecurityController(
            GetSecurityMatchesHandler getSecurityMatchesHandler,
            VerifyMemberAccessHandler verifyMemberAccessHandler,
            GetMatchAccessLogHandler getMatchAccessLogHandler)
        {
            _getSecurityMatchesHandler = getSecurityMatchesHandler;
            _verifyMemberAccessHandler = verifyMemberAccessHandler;
            _getMatchAccessLogHandler = getMatchAccessLogHandler;
        }

        [HttpGet("matches")]
        public async Task<IActionResult> GetMatches(CancellationToken cancellationToken)
        {
            var result = await _getSecurityMatchesHandler.HandleAsync(cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to retrieve security matches.",
                    result.Errors));
            }

            return Ok(ApiResponse<List<SecurityMatchResponse>>.Success(
                result.Data!,
                "Security matches retrieved successfully."));
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyMemberAccess(
            [FromBody] VerifyMemberAccessRequest request,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            if (userId is null)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    "Invalid token. User id was not found in token."));
            }

            var result = await _verifyMemberAccessHandler.HandleAsync(
                new VerifyMemberAccessCommand
                {
                    MatchId = request.MatchId,
                    MemberCode = request.MemberCode,
                    GateName = request.GateName,
                    ScannedByUserId = userId
                },
                cancellationToken);

            if (result.IsNotFound)
            {
                return NotFound(ApiResponse<object>.Fail(
                    "Match not found.",
                    result.Errors));
            }

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to verify member access.",
                    result.Errors));
            }

            return Ok(ApiResponse<VerifyMemberAccessResponse>.Success(
                result.Data!,
                result.Data!.IsAllowed
                    ? "Access allowed."
                    : "Access denied."));
        }

        [HttpGet("matches/{matchId:guid}/access-log")]
        public async Task<IActionResult> GetMatchAccessLog(
            Guid matchId,
            CancellationToken cancellationToken)
        {
            var result = await _getMatchAccessLogHandler.HandleAsync(
                new GetMatchAccessLogQuery { MatchId = matchId },
                cancellationToken);

            if (result.IsNotFound)
            {
                return NotFound(ApiResponse<object>.Fail(
                    "Match not found.",
                    result.Errors));
            }

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to retrieve match access log.",
                    result.Errors));
            }

            return Ok(ApiResponse<MatchAccessLogResponse>.Success(
                result.Data!,
                "Match access log retrieved successfully."));
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        }
    }

    public sealed class VerifyMemberAccessRequest
    {
        public Guid MatchId { get; set; }
        public string MemberCode { get; set; } = string.Empty;
        public string? GateName { get; set; }
    }
}
