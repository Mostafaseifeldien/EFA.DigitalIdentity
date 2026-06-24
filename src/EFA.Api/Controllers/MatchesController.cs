using EFA.Api.Common;
using EFA.Application.Matches.CreateMatch;
using EFA.Application.Matches.GetMatchById;
using EFA.Application.Matches.GetMatchLookups;
using EFA.Application.Matches.GetMatches;
using EFA.Application.Matches.UpdateMatch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFA.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,AssignmentsOfficer")]
    public sealed class MatchesController : ControllerBase
    {
        private readonly GetMatchesHandler _getMatchesHandler;
        private readonly CreateMatchHandler _createMatchHandler;
        private readonly GetMatchLookupsHandler _getMatchLookupsHandler;
        private readonly GetMatchByIdHandler _getMatchByIdHandler;
        private readonly UpdateMatchHandler _updateMatchHandler;

        public MatchesController(
            GetMatchesHandler getMatchesHandler,
            CreateMatchHandler createMatchHandler,
            GetMatchLookupsHandler getMatchLookupsHandler,
            GetMatchByIdHandler getMatchByIdHandler,
            UpdateMatchHandler updateMatchHandler)
        {
            _getMatchesHandler = getMatchesHandler;
            _createMatchHandler = createMatchHandler;
            _getMatchLookupsHandler = getMatchLookupsHandler;
            _getMatchByIdHandler = getMatchByIdHandler;
            _updateMatchHandler = updateMatchHandler;
        }

        [HttpGet]
        public async Task<IActionResult> GetMatches(
            [FromQuery] string? search,
            [FromQuery] string? tournament,
            CancellationToken cancellationToken)
        {
            var result = await _getMatchesHandler.HandleAsync(
                new GetMatchesQuery
                {
                    Search = search,
                    Tournament = tournament
                },
                cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to retrieve matches.",
                    result.Errors));
            }

            return Ok(ApiResponse<List<GetMatchesResponse>>.Success(
                result.Data!,
                "Matches retrieved successfully."));
        }

        [HttpGet("lookups")]
        public async Task<IActionResult> GetLookups(CancellationToken cancellationToken)
        {
            var data = await _getMatchLookupsHandler.HandleAsync(cancellationToken);

            return Ok(ApiResponse<GetMatchLookupsResponse>.Success(
                data,
                "Match lookups retrieved successfully."));
        }

        [HttpPost]
        public async Task<IActionResult> CreateMatch(
            [FromBody] CreateMatchRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _createMatchHandler.HandleAsync(request, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to create match.",
                    result.Errors));
            }

            return Ok(ApiResponse<CreateMatchResponse>.Success(
                result.Data!,
                "Match created and published successfully."));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetMatchById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _getMatchByIdHandler.HandleAsync(
                new GetMatchByIdQuery { Id = id },
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
                    "Failed to retrieve match.",
                    result.Errors));
            }

            return Ok(ApiResponse<GetMatchByIdResponse>.Success(
                result.Data!,
                "Match retrieved successfully."));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateMatch(
            Guid id,
            [FromBody] UpdateMatchCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _updateMatchHandler.HandleAsync(id, command, cancellationToken);

            if (result.IsNotFound)
            {
                return NotFound(ApiResponse<object>.Fail(
                    "Match not found.",
                    result.Errors));
            }

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to update match.",
                    result.Errors));
            }

            return Ok(ApiResponse<UpdateMatchResponse>.Success(
                result.Data!,
                "Match updated successfully."));
        }
    }
}
