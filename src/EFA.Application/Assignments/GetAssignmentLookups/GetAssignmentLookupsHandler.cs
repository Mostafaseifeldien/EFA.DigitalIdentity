using EFA.Application.Assignments.Common;
using EFA.Application.Common.Interfaces;
using EFA.Domain.Matches;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Assignments.GetAssignmentLookups
{
    public sealed class GetAssignmentLookupsHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetAssignmentLookupsHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
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
                .Select(x => new AssignmentLookupItemResponse
                {
                    Id = x.Id,
                    Name = x.FullName
                })
                .ToListAsync(cancellationToken);

            return new GetAssignmentLookupsResponse
            {
                Matches = matches.Select(x => new AssignmentLookupItemResponse
                {
                    Id = x.Id,
                    Name = AssignmentMatchHelper.GetMatchDisplayName(x)
                }).ToList(),
                Members = members,
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
