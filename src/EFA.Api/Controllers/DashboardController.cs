using EFA.Api.Common;
using EFA.Application.Dashboard;
using EFA.Application.Dashboard.GetDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFA.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "Admin,MembershipOfficer,AssignmentsOfficer,CommunicationsOfficer,ReportsViewer")]
    public sealed class DashboardController : ControllerBase
    {
        private readonly GetDashboardHandler _getDashboardHandler;

        public DashboardController(GetDashboardHandler getDashboardHandler)
        {
            _getDashboardHandler = getDashboardHandler;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
        {
            var result = await _getDashboardHandler.HandleAsync(cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to retrieve dashboard data.",
                    result.Errors));
            }

            return Ok(ApiResponse<DashboardResponse>.Success(
                result.Data!,
                "Dashboard data retrieved successfully."));
        }
    }
}
