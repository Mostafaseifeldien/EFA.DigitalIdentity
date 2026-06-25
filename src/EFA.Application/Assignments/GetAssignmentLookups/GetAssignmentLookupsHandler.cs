using EFA.Application.Assignments.Common;
using EFA.Application.Common.Interfaces;
using EFA.Domain.Matches;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Assignments.GetAssignmentLookups
{
    public sealed class GetAssignmentLookupsHandler
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IUserRoleReader _userRoleReader;

        public GetAssignmentLookupsHandler(
            IApplicationDbContext dbContext,
            IUserRoleReader userRoleReader)
        {
            _dbContext = dbContext;
            _userRoleReader = userRoleReader;
        }

        public async Task<GetAssignmentLookupsResponse> HandleAsync(CancellationToken cancellationToken = default)
        {
            var matches = await _dbContext.Matches
                .AsNoTracking()
                .Include(x => x.HomeTeam)
                .Include(x => x.AwayTeam)
                .Include(x => x.Stadium)
                .Where(x => x.Status != MatchStatus.Finished)
                .OrderByDescending(x => x.MatchDateTime)
                .ToListAsync(cancellationToken);

            var members = await _dbContext.Members
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.FullName)
                .ToListAsync(cancellationToken);

            var roleByUserId = await _userRoleReader.GetRoleNamesByUserIdsAsync(
                members.Select(x => x.UserId),
                cancellationToken);

            return new GetAssignmentLookupsResponse
            {
                Matches = matches.Select(x => new MatchLookupItemResponse
                {
                    Id = x.Id,
                    Name = AssignmentMatchHelper.GetMatchDisplayName(x),
                    MatchDateTime = x.MatchDateTime
                }).ToList(),
                Members = members.Select(x => new MemberLookupItemResponse
                {
                    Id = x.Id,
                    Name = x.FullName,
                    Role = roleByUserId.TryGetValue(x.UserId, out var role)
                        ? role
                        : string.Empty
                }).ToList(),
                AssignmentRoles = AssignmentRoleMappings.GetAllRoles()
                    .Select(x => new AssignmentRoleLookupItemResponse
                    {
                        Value = (int)x.Value,
                        Name = x.Name
                    })
                    .ToList()
            };
        }
    }
}
