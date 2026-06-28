using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EFA.Api.Common;
using EFA.Application.Members;
using EFA.Application.Members.CreateMember;
using EFA.Application.Members.GetMemberById;
using EFA.Application.Members.GetMembers;
using EFA.Application.Members.GetMyMemberProfile;
using EFA.Application.Members.ToggleMemberStatus;
using EFA.Application.Members.UpdateMember;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFA.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class MembersController : ControllerBase
    {
        private readonly CreateMemberHandler _createMemberHandler;
        private readonly GetMembersHandler _getMembersHandler;
        private readonly GetMemberByIdHandler _getMemberByIdHandler;
        private readonly GetMyMemberProfileHandler _getMyMemberProfileHandler;
        private readonly UpdateMemberHandler _updateMemberHandler;
        private readonly ToggleMemberStatusHandler _toggleMemberStatusHandler;

        public MembersController(
            CreateMemberHandler createMemberHandler,
            GetMembersHandler getMembersHandler,
            GetMemberByIdHandler getMemberByIdHandler,
            GetMyMemberProfileHandler getMyMemberProfileHandler,
            UpdateMemberHandler updateMemberHandler,
            ToggleMemberStatusHandler toggleMemberStatusHandler)
        {
            _createMemberHandler = createMemberHandler;
            _getMembersHandler = getMembersHandler;
            _getMemberByIdHandler = getMemberByIdHandler;
            _getMyMemberProfileHandler = getMyMemberProfileHandler;
            _updateMemberHandler = updateMemberHandler;
            _toggleMemberStatusHandler = toggleMemberStatusHandler;
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    "Invalid token. User id was not found in token."));
            }

            var result = await _getMyMemberProfileHandler.HandleAsync(
                new GetMyMemberProfileQuery { UserId = userId },
                cancellationToken);

            if (result.IsNotFound)
            {
                return NotFound(ApiResponse<object>.Fail(
                    "Member not found.",
                    result.Errors));
            }

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to retrieve member.",
                    result.Errors));
            }

            return Ok(ApiResponse<MemberDetailsResponse>.Success(
                result.Data!,
                "Member retrieved successfully."));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Member")]
        public async Task<IActionResult> GetMembers(
            [FromQuery] string? search,
            [FromQuery] string? memberType,
            [FromQuery] string? status,
            CancellationToken cancellationToken)
        {
            var result = await _getMembersHandler.HandleAsync(
                new GetMembersQuery
                {
                    Search = search,
                    MemberType = memberType,
                    Status = status
                },
                cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to retrieve members.",
                    result.Errors));
            }

            return Ok(ApiResponse<List<GetMembersResponse>>.Success(
                result.Data!,
                "Members retrieved successfully."));
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin,Member")]
        public async Task<IActionResult> GetMemberById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _getMemberByIdHandler.HandleAsync(
                new GetMemberByIdQuery { Id = id },
                cancellationToken);

            if (result.IsNotFound)
            {
                return NotFound(ApiResponse<object>.Fail(
                    "Member not found.",
                    result.Errors));
            }

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to retrieve member.",
                    result.Errors));
            }

            return Ok(ApiResponse<MemberDetailsResponse>.Success(
                result.Data!,
                "Member retrieved successfully."));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin,Member")]
        public async Task<IActionResult> UpdateMember(
            Guid id,
            [FromBody] UpdateMemberCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _updateMemberHandler.HandleAsync(id, command, cancellationToken);

            if (result.IsNotFound)
            {
                return NotFound(ApiResponse<object>.Fail(
                    "Member not found.",
                    result.Errors));
            }

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to update member.",
                    result.Errors));
            }

            return Ok(ApiResponse<MemberDetailsResponse>.Success(
                result.Data!,
                "Member updated successfully."));
        }

        [HttpPatch("{id:guid}/toggle-status")]
        [Authorize(Roles = "Admin,Member")]
        public async Task<IActionResult> ToggleMemberStatus(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _toggleMemberStatusHandler.HandleAsync(
                new ToggleMemberStatusCommand { Id = id },
                cancellationToken);

            if (result.IsNotFound)
            {
                return NotFound(ApiResponse<object>.Fail(
                    "Member not found.",
                    result.Errors));
            }

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to toggle member status.",
                    result.Errors));
            }

            return Ok(ApiResponse<MemberDetailsResponse>.Success(
                result.Data!,
                "Member status updated successfully."));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Member")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateMember([FromForm] CreateMemberRequest request)
        {
            var result = await _createMemberHandler.HandleAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to create member.",
                    result.Errors));
            }

            return Ok(ApiResponse<CreateMemberResponse>.Success(
                result.Data!,
                "Member created successfully."));
        }
    }
}
