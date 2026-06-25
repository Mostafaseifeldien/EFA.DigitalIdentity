using EFA.Application.Assignments.Common;
using EFA.Application.Common.Interfaces;
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

            var member = await ResolveMemberAsync(query, cancellationToken);

            if (member is null)
            {
                var notFoundMessage = query.MemberId.HasValue
                    ? "Member was not found."
                    : "Member profile was not found for the current user.";

                return (false, null, new List<string> { notFoundMessage }, true);
            }

            var response = await GetAssignmentsForMemberAsync(member.Id, cancellationToken);

            return (true, response, new List<string>(), false);
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
            CancellationToken cancellationToken)
        {
            var assignments = await _dbContext.Assignments
                .AsNoTracking()
                .Include(x => x.Match)
                    .ThenInclude(x => x.Tournament)
                .Include(x => x.Match)
                    .ThenInclude(x => x.HomeTeam)
                .Include(x => x.Match)
                    .ThenInclude(x => x.AwayTeam)
                .Include(x => x.Match)
                    .ThenInclude(x => x.Stadium)
                .Where(x => x.MemberId == memberId)
                .OrderBy(x => x.Match.MatchDateTime)
                .ToListAsync(cancellationToken);

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
    }
}
