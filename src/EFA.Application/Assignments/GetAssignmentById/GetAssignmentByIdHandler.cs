using EFA.Application.Assignments.Common;
using EFA.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Assignments.GetAssignmentById
{
    public sealed class GetAssignmentByIdHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetAssignmentByIdHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool IsSuccess, AssignmentDetailsResponse? Data, List<string> Errors, bool IsNotFound)> HandleAsync(
            GetAssignmentByIdQuery query,
            CancellationToken cancellationToken = default)
        {
            var assignment = await _dbContext.Assignments
                .AsNoTracking()
                .Include(x => x.Member)
                .Include(x => x.Match)
                    .ThenInclude(x => x.HomeTeam)
                .Include(x => x.Match)
                    .ThenInclude(x => x.AwayTeam)
                .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);

            if (assignment is null)
            {
                return (false, null, new List<string> { "Assignment not found." }, true);
            }

            return (true, AssignmentResponseMapper.MapToDetails(assignment), new List<string>(), false);
        }
    }
}
