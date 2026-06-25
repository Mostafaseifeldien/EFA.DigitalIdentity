using EFA.Application.Assignments.Common;
using EFA.Application.Common.Interfaces;
using EFA.Domain.Assignments;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Assignments.GetAssignments
{
    public sealed class GetAssignmentsHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetAssignmentsHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool IsSuccess, List<GetAssignmentsResponse>? Data, List<string> Errors)> HandleAsync(
            GetAssignmentsQuery query,
            CancellationToken cancellationToken = default)
        {
            var assignmentsQuery = _dbContext.Assignments
                .AsNoTracking()
                .Include(x => x.Member)
                .Include(x => x.Match)
                    .ThenInclude(x => x.HomeTeam)
                .Include(x => x.Match)
                    .ThenInclude(x => x.AwayTeam)
                .AsQueryable();

            if (query.MatchId.HasValue)
            {
                assignmentsQuery = assignmentsQuery.Where(x => x.MatchId == query.MatchId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();
                assignmentsQuery = assignmentsQuery.Where(x =>
                    x.Member.FullName.Contains(search) ||
                    (x.Match.HomeTeam.Name + " × " + x.Match.AwayTeam.Name).Contains(search));
            }

            var assignments = await assignmentsQuery
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            var response = assignments.Select(x =>
            {
                var canModify = x.Status != AssignmentStatus.Cancelled;

                return new GetAssignmentsResponse
                {
                    Id = x.Id,
                    MemberId = x.MemberId,
                    MemberName = x.Member.FullName,
                    MatchId = x.MatchId,
                    MatchName = AssignmentMatchHelper.GetMatchName(x.Match),
                    AssignmentRole = x.AssignmentRole,
                    AssignmentRoleName = AssignmentRoleMappings.GetArabicName(x.AssignmentRole),
                    Status = x.Status,
                    StatusName = AssignmentStatusMappings.GetArabicName(x.Status),
                    HasConflict = x.HasConflict,
                    ConflictMessage = x.ConflictMessage,
                    AssignmentDate = x.CreatedAt,
                    CanEdit = canModify,
                    CanCancel = canModify
                };
            }).ToList();

            return (true, response, new List<string>());
        }
    }
}
