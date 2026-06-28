using EFA.Api.Common;
using EFA.Application.Players;
using EFA.Application.Players.CreatePlayer;
using EFA.Application.Players.GetPlayerById;
using EFA.Application.Players.GetPlayers;
using EFA.Application.Players.UpdatePlayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFA.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,MembershipOfficer")]
    public sealed class PlayersController : ControllerBase
    {
        private readonly CreatePlayerHandler _createPlayerHandler;
        private readonly GetPlayersHandler _getPlayersHandler;
        private readonly GetPlayerByIdHandler _getPlayerByIdHandler;
        private readonly UpdatePlayerHandler _updatePlayerHandler;

        public PlayersController(
            CreatePlayerHandler createPlayerHandler,
            GetPlayersHandler getPlayersHandler,
            GetPlayerByIdHandler getPlayerByIdHandler,
            UpdatePlayerHandler updatePlayerHandler)
        {
            _createPlayerHandler = createPlayerHandler;
            _getPlayersHandler = getPlayersHandler;
            _getPlayerByIdHandler = getPlayerByIdHandler;
            _updatePlayerHandler = updatePlayerHandler;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePlayer(
            [FromBody] CreatePlayerCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _createPlayerHandler.HandleAsync(command, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to create player.",
                    result.Errors));
            }

            return Ok(ApiResponse<PlayerResponse>.Success(
                result.Data!,
                "Player created successfully."));
        }

        [HttpGet]
        public async Task<IActionResult> GetPlayers(
            [FromQuery] string? playerName,
            [FromQuery] string? clubName,
            CancellationToken cancellationToken)
        {
            var result = await _getPlayersHandler.HandleAsync(
                new GetPlayersQuery
                {
                    PlayerName = playerName,
                    ClubName = clubName
                },
                cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to retrieve players.",
                    result.Errors));
            }

            return Ok(ApiResponse<List<PlayerResponse>>.Success(
                result.Data!,
                "Players retrieved successfully."));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPlayerById(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _getPlayerByIdHandler.HandleAsync(
                new GetPlayerByIdQuery { Id = id },
                cancellationToken);

            if (result.IsNotFound)
            {
                return NotFound(ApiResponse<object>.Fail(
                    "Player not found.",
                    result.Errors));
            }

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to retrieve player.",
                    result.Errors));
            }

            return Ok(ApiResponse<PlayerResponse>.Success(
                result.Data!,
                "Player retrieved successfully."));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePlayer(
            int id,
            [FromBody] UpdatePlayerCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _updatePlayerHandler.HandleAsync(id, command, cancellationToken);

            if (result.IsNotFound)
            {
                return NotFound(ApiResponse<object>.Fail(
                    "Player not found.",
                    result.Errors));
            }

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Failed to update player.",
                    result.Errors));
            }

            return Ok(ApiResponse<PlayerResponse>.Success(
                result.Data!,
                "Player updated successfully."));
        }
    }
}
