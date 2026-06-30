using EFA.Application.Assignments.Common;
using EFA.Application.Common.Interfaces;
using EFA.Application.Security.Common;
using EFA.Domain.Assignments;
using EFA.Domain.Matches;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Security.GetSecurityMatches
{
    public sealed class GetSecurityMatchesHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetSecurityMatchesHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool IsSuccess, List<SecurityMatchResponse>? Data, List<string> Errors)> HandleAsync(
            GetSecurityMatchesQuery query,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query.UserId))
            {
                return (false, null, new List<string> { "User id is required." });
            }

            var member = await _dbContext.Members
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == query.UserId, cancellationToken);

            if (member is null)
            {
                return (true, new List<SecurityMatchResponse>(), new List<string>());
            }

            var assignedMatchIds = await _dbContext.Assignments
                .AsNoTracking()
                .Where(x =>
                    x.MemberId == member.Id &&
                    x.Status != AssignmentStatus.Cancelled)
                .Select(x => x.MatchId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (assignedMatchIds.Count == 0)
            {
                return (true, new List<SecurityMatchResponse>(), new List<string>());
            }

            var today = DateTime.Now.Date;

            var matches = await _dbContext.Matches
                .AsNoTracking()
                .Include(x => x.HomeTeam)
                .Include(x => x.AwayTeam)
                .Include(x => x.Stadium)
                .Where(x =>
                    assignedMatchIds.Contains(x.Id) &&
                    x.Status != MatchStatus.Finished &&
                    x.MatchDateTime.Date >= today)
                .OrderBy(x => x.MatchDateTime)
                .ToListAsync(cancellationToken);

            var response = matches
                .Select(match =>
                {
                    var schedule = SecurityDisplayHelper.GetMatchScheduleStatus(
                        match.MatchDateTime,
                        match.Status == MatchStatus.Finished);

                    return new SecurityMatchResponse
                    {
                        Id = match.Id,
                        MatchName = AssignmentMatchHelper.GetMatchName(match),
                        StadiumName = match.Stadium.Name,
                        MatchDateTime = match.MatchDateTime,
                        ScheduleStatus = schedule.Status,
                        ScheduleStatusName = schedule.StatusName,
                        DisplaySubtitle = schedule.Status == "Live"
                            ? $"{match.Stadium.Name} — البوابة 4"
                            : $"{match.Stadium.Name} — {match.MatchDateTime:dddd d MMMM}"
                    };
                })
                .ToList();

            return (true, response, new List<string>());
        }
    }
}
