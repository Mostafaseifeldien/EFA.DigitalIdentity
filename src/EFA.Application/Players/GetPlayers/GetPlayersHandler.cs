using EFA.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Players.GetPlayers
{
    public sealed class GetPlayersHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetPlayersHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool IsSuccess, List<PlayerResponse>? Data, List<string> Errors)> HandleAsync(
            GetPlayersQuery query,
            CancellationToken cancellationToken = default)
        {
            var playersQuery = _dbContext.Players.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.PlayerName))
            {
                var playerName = query.PlayerName.Trim();
                playersQuery = playersQuery.Where(x => x.FullName.Contains(playerName));
            }

            if (!string.IsNullOrWhiteSpace(query.ClubName))
            {
                var clubName = query.ClubName.Trim();
                playersQuery = playersQuery.Where(x => x.ClubName.Contains(clubName));
            }

            var players = await playersQuery
                .OrderBy(x => x.FullName)
                .ToListAsync(cancellationToken);

            var response = players.Select(PlayerResponse.MapFrom).ToList();

            return (true, response, new List<string>());
        }
    }
}
