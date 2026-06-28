using EFA.Application.Assignments.Common;
using EFA.Application.Common.Interfaces;
using EFA.Domain.Assignments;
using EFA.Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Assignments.GetMyAssignments
{
    public sealed class GetMyAssignmentsHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetMyAssignmentsHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool IsSuccess, List<MyAssignmentResponse>? Data, List<string> Errors, bool IsNotFound)> HandleAsync(
            GetMyAssignmentsQuery query,
            CancellationToken cancellationToken = default)
        {
            if (!query.MemberId.HasValue && string.IsNullOrWhiteSpace(query.UserId))
            {
                return (false, null, new List<string> { "Either userId or memberId is required." }, false);
            }

            if (!TryValidateFilters(query, out var filter, out var validationErrors))
            {
                return (false, null, validationErrors, false);
            }

            var member = await ResolveMemberAsync(query, cancellationToken);

            if (member is null)
            {
                var notFoundMessage = query.MemberId.HasValue
                    ? "Member was not found."
                    : "Member profile was not found for the current user.";

                return (false, null, new List<string> { notFoundMessage }, true);
            }

            var response = await GetAssignmentsForMemberAsync(member.Id, filter, cancellationToken);

            return (true, response, new List<string>(), false);
        }

        private static bool TryValidateFilters(
            GetMyAssignmentsQuery query,
            out MyAssignmentsFilter filter,
            out List<string> errors)
        {
            filter = new MyAssignmentsFilter();
            errors = new List<string>();

            if (!string.IsNullOrWhiteSpace(query.Type))
            {
                var type = query.Type.Trim();

                if (type is not ("next" or "previous"))
                {
                    errors.Add("type must be one of: next, previous");
                    return false;
                }

                filter.Type = type;
            }

            if (!string.IsNullOrWhiteSpace(query.Period))
            {
                var period = query.Period.Trim();

                if (!string.Equals(period, "today", StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add("period must be: today");
                    return false;
                }

                filter.Period = "today";
            }

            return true;
        }

        private async Task<Member?> ResolveMemberAsync(
            GetMyAssignmentsQuery query,
            CancellationToken cancellationToken)
        {
            if (query.MemberId.HasValue)
            {
                return await _dbContext.Members
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == query.MemberId.Value, cancellationToken);
            }

            if (string.IsNullOrWhiteSpace(query.UserId))
            {
                return null;
            }

            return await _dbContext.Members
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == query.UserId, cancellationToken);
        }

        private async Task<List<MyAssignmentResponse>> GetAssignmentsForMemberAsync(
            Guid memberId,
            MyAssignmentsFilter filter,
            CancellationToken cancellationToken)
        {
            var now = DateTime.Now;
            var today = now.Date;

            var assignmentsQuery = _dbContext.Assignments
                .AsNoTracking()
                .Include(x => x.Match)
                    .ThenInclude(x => x.Tournament)
                .Include(x => x.Match)
                    .ThenInclude(x => x.HomeTeam)
                .Include(x => x.Match)
                    .ThenInclude(x => x.AwayTeam)
                .Include(x => x.Match)
                    .ThenInclude(x => x.Stadium)
                .Where(x =>
                    x.MemberId == memberId &&
                    x.Status != AssignmentStatus.Cancelled);

            if (filter.Period == "today")
            {
                assignmentsQuery = assignmentsQuery.Where(x =>
                    x.Match.MatchDateTime.Date == today &&
                    x.Match.MatchDateTime >= now);
            }
            else if (filter.Type == "next")
            {
                assignmentsQuery = assignmentsQuery.Where(x => x.Match.MatchDateTime >= now);
            }
            else if (filter.Type == "previous")
            {
                assignmentsQuery = assignmentsQuery.Where(x => x.Match.MatchDateTime < now);
            }

            var orderDescending = filter.Type == "previous" && filter.Period != "today";

            assignmentsQuery = orderDescending
                ? assignmentsQuery.OrderByDescending(x => x.Match.MatchDateTime)
                : assignmentsQuery.OrderBy(x => x.Match.MatchDateTime);

            var assignments = await assignmentsQuery.ToListAsync(cancellationToken);

            return assignments.Select(x => new MyAssignmentResponse
            {
                AssignmentId = x.Id,
                MatchId = x.MatchId,
                MatchName = AssignmentMatchHelper.GetMatchName(x.Match),
                TournamentName = x.Match.Tournament.Name,
                StadiumName = x.Match.Stadium.Name,
                MatchDateTime = x.Match.MatchDateTime,
                MatchDateTimeText = FormatMatchDateTimeText(x.Match.MatchDateTime),
                AssignmentRole = x.AssignmentRole,
                AssignmentRoleName = AssignmentRoleMappings.GetDisplayName(x),
                Status = x.Status,
                StatusName = AssignmentStatusMappings.GetArabicName(x.Status),
                HasConflict = x.HasConflict,
                ConflictMessage = x.ConflictMessage,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                CancelledAt = x.CancelledAt
            }).ToList();
        }

        private static string FormatMatchDateTimeText(DateTime matchDateTime)
        {
            return matchDateTime.ToString("dd/MM/yyyy — hh:mm tt");
        }

        private sealed class MyAssignmentsFilter
        {
            public string? Type { get; set; }
            public string? Period { get; set; }
        }
    }
}
