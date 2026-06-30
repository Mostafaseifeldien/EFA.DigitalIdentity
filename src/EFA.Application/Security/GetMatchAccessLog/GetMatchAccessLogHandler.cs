using EFA.Application.Assignments.Common;
using EFA.Application.Common.Interfaces;
using EFA.Application.Security.Common;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Security.GetMatchAccessLog
{
    public sealed class GetMatchAccessLogHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetMatchAccessLogHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool IsSuccess, MatchAccessLogResponse? Data, List<string> Errors, bool IsNotFound)> HandleAsync(
            GetMatchAccessLogQuery query,
            CancellationToken cancellationToken = default)
        {
            var match = await _dbContext.Matches
                .AsNoTracking()
                .Include(x => x.HomeTeam)
                .Include(x => x.AwayTeam)
                .FirstOrDefaultAsync(x => x.Id == query.MatchId, cancellationToken);

            if (match is null)
            {
                return (false, null, new List<string> { "Match not found." }, true);
            }

            var logs = await _dbContext.MatchAccessLogs
                .AsNoTracking()
                .Include(x => x.Member)
                .Where(x => x.MatchId == query.MatchId)
                .OrderByDescending(x => x.ScannedAt)
                .ToListAsync(cancellationToken);

            var entries = logs
                .Select(log => new MatchAccessLogEntryResponse
                {
                    Id = log.Id,
                    FullName = log.Member?.FullName,
                    MemberCode = log.MemberCode,
                    Initials = SecurityDisplayHelper.GetInitials(log.Member?.FullName),
                    DisplayText = log.IsAllowed
                        ? log.AssignmentRoleName ?? "—"
                        : log.RejectionReasonName ?? SecurityVerificationConstants.GetRejectionReasonName(log.RejectionReasonCode),
                    ScannedAt = log.ScannedAt,
                    IsAllowed = log.IsAllowed,
                    StatusName = log.IsAllowed ? "سماح" : "رفض",
                    RejectionReasonName = log.IsAllowed ? null : log.RejectionReasonName
                })
                .ToList();

            return (true, new MatchAccessLogResponse
            {
                MatchId = match.Id,
                MatchName = AssignmentMatchHelper.GetMatchName(match),
                TotalScans = logs.Count,
                AllowedCount = logs.Count(x => x.IsAllowed),
                RejectedCount = logs.Count(x => !x.IsAllowed),
                Entries = entries
            }, new List<string>(), false);
        }
    }
}
