using EFA.Api.Common;
using EFA.Application.Members.CreateMember;
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

        public MembersController(CreateMemberHandler createMemberHandler)
        {
            _createMemberHandler = createMemberHandler;
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
