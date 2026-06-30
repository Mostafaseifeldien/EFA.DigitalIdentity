using EFA.Api.Common;
using EFA.Application.Assignments;
using EFA.Application.Assignments.BulkCreateAssignments;
using EFA.Application.Assignments.CancelAssignment;
using EFA.Application.Assignments.Common;
using EFA.Application.Assignments.GetAssignmentById;
using EFA.Application.Assignments.GetAssignmentLookups;
using EFA.Application.Assignments.GetAssignments;
using EFA.Application.Assignments.GetMyAssignments;
using EFA.Application.Assignments.UpdateAssignment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFA.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "Admin,AssignmentsOfficer")]
    public sealed class AssignmentsController : ControllerBase
    {
        private readonly GetAssignmentsHandler _getAssignmentsHandler;
        private readonly GetAssignmentLookupsHandler _getAssignmentLookupsHandler;
        private readonly BulkCreateAssignmentsHandler _bulkCreateAssignmentsHandler;
        private readonly GetAssignmentByIdHandler _getAssignmentByIdHandler;
        private readonly UpdateAssignmentHandler _updateAssignmentHandler;
        private readonly CancelAssignmentHandler _cancelAssignmentHandler;
        private readonly GetMyAssignmentsHandler _getMyAssignmentsHandler;

        public AssignmentsController(
            GetAssignmentsHandler getAssignmentsHandler,
            GetAssignmentLookupsHandler getAssignmentLookupsHandler,
            BulkCreateAssignmentsHandler bulkCreateAssignmentsHandler,
            GetAssignmentByIdHandler getAssignmentByIdHandler,
            UpdateAssignmentHandler updateAssignmentHandler,
            CancelAssignmentHandler cancelAssignmentHandler,
            GetMyAssignmentsHandler getMyAssignmentsHandler)
        {
            _getAssignmentsHandler = getAssignmentsHandler;
            _getAssignmentLookupsHandler = getAssignmentLookupsHandler;
            _bulkCreateAssignmentsHandler = bulkCreateAssignmentsHandler;
            _getAssignmentByIdHandler = getAssignmentByIdHandler;
            _updateAssignmentHandler = updateAssignmentHandler;
            _cancelAssignmentHandler = cancelAssignmentHandler;
            _getMyAssignmentsHandler = getMyAssignmentsHandler;
        }

        [HttpGet]
        public async Task<IActionResult> GetAssignments(
            [FromQuery] Guid? matchId,
            [FromQuery] string? search,
            CancellationToken cancellationToken)
        {
            var result = await _getAssignmentsHandler.HandleAsync(
                new GetAssignmentsQuery
                {
                    MatchId = matchId,
                    Search = search
                },
                cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to retrieve assignments.",
                    result.Errors));
            }

            return Ok(ApiResponse<List<GetAssignmentsResponse>>.Success(
                result.Data!,
                "Assignments retrieved successfully."));
        }

        [HttpGet("member/{memberId:guid}")]
        public async Task<IActionResult> GetMemberAssignments(
            Guid memberId,
            CancellationToken cancellationToken)
        {
            var result = await _getMyAssignmentsHandler.HandleAsync(
                new GetMyAssignmentsQuery { MemberId = memberId },
                cancellationToken);

            if (result.IsNotFound)
            {
                return NotFound(ApiResponse<object>.Fail(
                    "Member was not found.",
                    result.Errors));
            }

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to retrieve member assignments.",
                    result.Errors));
            }

            return Ok(ApiResponse<List<MyAssignmentResponse>>.Success(
                result.Data!,
                "Member assignments retrieved successfully."));
        }

        [HttpGet("lookups")]
        public async Task<IActionResult> GetLookups(CancellationToken cancellationToken)
        {
            var data = await _getAssignmentLookupsHandler.HandleAsync(cancellationToken);

            return Ok(ApiResponse<GetAssignmentLookupsResponse>.Success(
                data,
                "Assignment lookups retrieved successfully."));
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreateAssignments(
            [FromBody] BulkCreateAssignmentsRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _bulkCreateAssignmentsHandler.HandleAsync(request, cancellationToken);

            if (result.IsConflict)
            {
                return Conflict(new ApiResponse<BulkCreateAssignmentsConflictResponse>
                {
                    IsSuccess = false,
                    Message = "Assignment conflicts detected.",
                    Data = result.Conflicts,
                    Errors = result.Errors
                });
            }

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    result.Errors.FirstOrDefault() ?? "Failed to create assignments.",
                    result.Errors));
            }

            return Ok(ApiResponse<BulkCreateAssignmentsResponse>.Success(
                result.Data!,
                "Assignments created successfully."));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetAssignmentById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _getAssignmentByIdHandler.HandleAsync(
                new GetAssignmentByIdQuery { Id = id },
                cancellationToken);

            if (result.IsNotFound)
            {
                return NotFound(ApiResponse<object>.Fail(
                    "Assignment not found.",
                    result.Errors));
            }

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to retrieve assignment.",
                    result.Errors));
            }

            return Ok(ApiResponse<AssignmentDetailsResponse>.Success(
                result.Data!,
                "Assignment retrieved successfully."));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAssignment(
            Guid id,
            [FromBody] UpdateAssignmentCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _updateAssignmentHandler.HandleAsync(id, command, cancellationToken);

            if (result.IsNotFound)
            {
                return NotFound(ApiResponse<object>.Fail(
                    "Assignment not found.",
                    result.Errors));
            }

            if (result.IsModificationBlocked)
            {
                return Conflict(ApiResponse<object>.Fail(
                    AssignmentModificationRules.ModificationNotAllowedMessage,
                    result.Errors));
            }

            if (result.IsConflict)
            {
                return Conflict(new ApiResponse<BulkCreateAssignmentsConflictResponse>
                {
                    IsSuccess = false,
                    Message = "Assignment conflict detected.",
                    Data = result.Conflicts,
                    Errors = result.Errors
                });
            }

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to update assignment.",
                    result.Errors));
            }

            return Ok(ApiResponse<AssignmentDetailsResponse>.Success(
                result.Data!,
                "Assignment updated successfully."));
        }

        [HttpPatch("{id:guid}/cancel")]
        public async Task<IActionResult> CancelAssignment(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _cancelAssignmentHandler.HandleAsync(
                new CancelAssignmentCommand { Id = id },
                cancellationToken);

            if (result.IsNotFound)
            {
                return NotFound(ApiResponse<object>.Fail(
                    "Assignment not found.",
                    result.Errors));
            }

            if (result.IsModificationBlocked)
            {
                return Conflict(ApiResponse<object>.Fail(
                    AssignmentModificationRules.CancellationNotAllowedMessage,
                    result.Errors));
            }

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to cancel assignment.",
                    result.Errors));
            }

            return Ok(ApiResponse<AssignmentDetailsResponse>.Success(
                result.Data!,
                "Assignment cancelled successfully."));
        }
    }
}
