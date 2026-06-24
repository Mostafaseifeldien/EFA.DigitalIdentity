using EFA.Application.Common.Interfaces;
using EFA.Application.Matches.Common;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Matches.GetMatchById
{
    public sealed class GetMatchByIdHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetMatchByIdHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool IsSuccess, GetMatchByIdResponse? Data, List<string> Errors, bool IsNotFound)> HandleAsync(
            GetMatchByIdQuery query,
            CancellationToken cancellationToken = default)
        {
            var match = await _dbContext.Matches
                .AsNoTracking()
                .Include(x => x.Tournament)
                .Include(x => x.HomeTeam)
                .Include(x => x.AwayTeam)
                .Include(x => x.Stadium)
                .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);

            if (match is null)
            {
                return (false, null, new List<string> { "Match not found." }, true);
            }

            var response = new GetMatchByIdResponse
            {
                Id = match.Id,
                TournamentName = match.Tournament.Name,
                RoundName = match.Round,
                FirstTeamName = match.HomeTeam.Name,
                SecondTeamName = match.AwayTeam.Name,
                StadiumName = match.Stadium.Name,
                MatchDateTime = match.MatchDateTime,
                Status = match.Status,
                StatusName = MatchStatusMappings.GetArabicName(match.Status),
                Notes = match.Notes
            };

            return (true, response, new List<string>(), false);
        }
    }
}
