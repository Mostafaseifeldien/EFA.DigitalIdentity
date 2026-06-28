using EFA.Application.Assignments.Common;
using EFA.Application.Common.Interfaces;
using EFA.Application.Matches.Common;
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
                .Include(x => x.Match)
                    .ThenInclude(x => x.Tournament)
                .Include(x => x.Match)
                    .ThenInclude(x => x.Stadium)
                .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);

            if (assignment is null)
            {
                return (false, null, new List<string> { "Assignment not found." }, true);
            }

            var response = AssignmentResponseMapper.MapToDetails(assignment);
            response.TournamentName = assignment.Match.Tournament.Name;
            response.RoundName = assignment.Match.Round;
            response.StadiumName = assignment.Match.Stadium.Name;
            response.MatchStatusName = MatchStatusMappings.GetArabicName(assignment.Match.Status);

            return (true, response, new List<string>(), false);
        }
    }
}
