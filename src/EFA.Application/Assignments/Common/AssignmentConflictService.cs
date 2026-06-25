using EFA.Application.Common.Interfaces;
using EFA.Domain.Assignments;
using EFA.Domain.Matches;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Assignments.Common
{
    public static class AssignmentConflictService
    {
        public static string BuildConflictMessage(string memberName)
        {
            return $"تعارض تشغيل: العضو {memberName} مكلف بالفعل في مباراة أخرى بنفس التوقيت";
        }

        public static async Task<AssignmentConflictDetail?> DetectConflictAsync(
            IApplicationDbContext dbContext,
            Guid memberId,
            string memberName,
            Guid matchId,
            DateTime matchDateTime,
            Guid? excludeAssignmentId = null,
            CancellationToken cancellationToken = default)
        {
            var conflictingAssignment = await dbContext.Assignments
                .AsNoTracking()
                .Include(x => x.Match)
                    .ThenInclude(x => x.HomeTeam)
                .Include(x => x.Match)
                    .ThenInclude(x => x.AwayTeam)
                .Where(x =>
                    x.MemberId == memberId &&
                    x.MatchId != matchId &&
                    x.Status != AssignmentStatus.Cancelled &&
                    x.Match.MatchDateTime == matchDateTime &&
                    (excludeAssignmentId == null || x.Id != excludeAssignmentId))
                .FirstOrDefaultAsync(cancellationToken);

            if (conflictingAssignment is null)
            {
                return null;
            }

            return new AssignmentConflictDetail
            {
                MemberId = memberId,
                MemberName = memberName,
                ConflictingMatchId = conflictingAssignment.MatchId,
                ConflictingMatchName = AssignmentMatchHelper.GetMatchName(conflictingAssignment.Match),
                Message = BuildConflictMessage(memberName)
            };
        }

        public static async Task<List<AssignmentConflictDetail>> DetectConflictsForMembersAsync(
            IApplicationDbContext dbContext,
            Match match,
            IEnumerable<(Guid MemberId, string MemberName)> members,
            Guid? excludeAssignmentId = null,
            CancellationToken cancellationToken = default)
        {
            var conflicts = new List<AssignmentConflictDetail>();

            foreach (var member in members.DistinctBy(x => x.MemberId))
            {
                var conflict = await DetectConflictAsync(
                    dbContext,
                    member.MemberId,
                    member.MemberName,
                    match.Id,
                    match.MatchDateTime,
                    excludeAssignmentId,
                    cancellationToken);

                if (conflict is not null)
                {
                    conflicts.Add(conflict);
                }
            }

            return conflicts;
        }
    }
}
