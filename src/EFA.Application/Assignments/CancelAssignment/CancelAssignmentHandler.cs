using EFA.Application.Assignments.Common;
using EFA.Application.Common.Interfaces;
using EFA.Domain.Assignments;
using EFA.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Assignments.CancelAssignment
{
    public sealed class CancelAssignmentHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public CancelAssignmentHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool IsSuccess, AssignmentDetailsResponse? Data, List<string> Errors, bool IsNotFound)> HandleAsync(
            CancelAssignmentCommand command,
            CancellationToken cancellationToken = default)
        {
            var assignment = await _dbContext.Assignments
                .Include(x => x.Member)
                .Include(x => x.Match)
                    .ThenInclude(x => x.HomeTeam)
                .Include(x => x.Match)
                    .ThenInclude(x => x.AwayTeam)
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (assignment is null)
            {
                return (false, null, new List<string> { "Assignment not found." }, true);
            }

            if (assignment.Status == AssignmentStatus.Cancelled)
            {
                return (false, null, new List<string> { "Assignment is already cancelled." }, false);
            }

            assignment.Status = AssignmentStatus.Cancelled;
            assignment.CancelledAt = DateTime.UtcNow;
            assignment.UpdatedAt = DateTime.UtcNow;

            AssignmentNotificationService.CreateAssignmentNotification(
                _dbContext,
                assignment.Member.UserId,
                NotificationType.AssignmentCancelled,
                AssignmentNotificationService.BuildCancelledMessage(
                    AssignmentMatchHelper.GetMatchName(assignment.Match),
                    assignment.AssignmentRole));

            await _dbContext.SaveChangesAsync(cancellationToken);

            return (true, AssignmentResponseMapper.MapToDetails(assignment), new List<string>(), false);
        }
    }
}
