using EFA.Api.Common;
using EFA.Application.Members.CreateMember;
using EFA.Application.Members.GetMembers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFA.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public sealed class MembersController : ControllerBase
    {
        private readonly CreateMemberHandler _createMemberHandler;
        private readonly GetMembersHandler _getMembersHandler;

        public MembersController(
            CreateMemberHandler createMemberHandler,
            GetMembersHandler getMembersHandler)
        {
            _createMemberHandler = createMemberHandler;
            _getMembersHandler = getMembersHandler;
        }

        [HttpGet]
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

        [HttpPost]
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
