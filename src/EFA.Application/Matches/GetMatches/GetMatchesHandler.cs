using EFA.Application.Common.Interfaces;
using EFA.Domain.Matches;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Matches.GetMatches
{
    public sealed class GetMatchesHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetMatchesHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool IsSuccess, List<GetMatchesResponse>? Data, List<string> Errors)> HandleAsync(
            GetMatchesQuery query,
            CancellationToken cancellationToken = default)
        {
            if (!TryParseTournamentFilter(query.Tournament, out var tournamentIdFilter, out var tournamentNameFilter, out var tournamentError))
            {
                return (false, null, new List<string> { tournamentError! });
            }

            var matchesQuery = _dbContext.Matches
                .AsNoTracking()
                .Include(x => x.Tournament)
                .Include(x => x.HomeTeam)
                .Include(x => x.AwayTeam)
                .Include(x => x.Stadium)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();
                matchesQuery = matchesQuery.Where(x =>
                    x.HomeTeam.Name.Contains(search) ||
                    x.AwayTeam.Name.Contains(search) ||
                    x.Tournament.Name.Contains(search) ||
                    (x.HomeTeam.Name + " × " + x.AwayTeam.Name).Contains(search));
            }

            if (tournamentIdFilter.HasValue)
            {
                matchesQuery = matchesQuery.Where(x => x.TournamentId == tournamentIdFilter.Value);
            }
            else if (!string.IsNullOrWhiteSpace(tournamentNameFilter))
            {
                matchesQuery = matchesQuery.Where(x => x.Tournament.Name == tournamentNameFilter);
            }

            var matches = await matchesQuery
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            var response = matches.Select(x => new GetMatchesResponse
            {
                Id = x.Id,
                MatchName = $"{x.HomeTeam.Name} × {x.AwayTeam.Name}",
                HomeTeamName = x.HomeTeam.Name,
                AwayTeamName = x.AwayTeam.Name,
                TournamentId = x.TournamentId,
                TournamentName = x.Tournament.Name,
                Round = x.Round,
                StadiumId = x.StadiumId,
                StadiumName = x.Stadium.Name,
                MatchDateTime = x.MatchDateTime,
                Status = x.Status,
                StatusName = GetStatusArabicName(x.Status),
                IsPublished = x.IsPublished,
                CreatedAt = x.CreatedAt
            }).ToList();

            return (true, response, new List<string>());
        }

        private static bool TryParseTournamentFilter(
            string? tournament,
            out int? tournamentIdFilter,
            out string? tournamentNameFilter,
            out string? error)
        {
            tournamentIdFilter = null;
            tournamentNameFilter = null;
            error = null;

            if (string.IsNullOrWhiteSpace(tournament) || tournament.Trim() == "كل البطولات")
            {
                return true;
            }

            var value = tournament.Trim();

            if (int.TryParse(value, out var tournamentId) && tournamentId > 0)
            {
                tournamentIdFilter = tournamentId;
                return true;
            }

            tournamentNameFilter = value;
            return true;
        }

        private static string GetStatusArabicName(MatchStatus status)
        {
            return status switch
            {
                MatchStatus.Confirmed => "مؤكدة",
                MatchStatus.UnderPreparation => "تحت الإعداد",
                MatchStatus.Finished => "منتهية",
                _ => "غير معروف"
            };
        }
    }
}
