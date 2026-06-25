using EFA.Application.Assignments.BulkCreateAssignments;
using EFA.Application.Assignments.Common;
using EFA.Application.Common.Interfaces;
using EFA.Domain.Assignments;
using EFA.Domain.Notifications;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Assignments.UpdateAssignment
{
    public sealed class UpdateAssignmentHandler
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IValidator<UpdateAssignmentCommand> _validator;

        public UpdateAssignmentHandler(
            IApplicationDbContext dbContext,
            IValidator<UpdateAssignmentCommand> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<(
            bool IsSuccess,
            AssignmentDetailsResponse? Data,
            BulkCreateAssignmentsConflictResponse? Conflicts,
            List<string> Errors,
            bool IsNotFound,
            bool IsConflict)> HandleAsync(
            Guid id,
            UpdateAssignmentCommand command,
            CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);

            if (!validationResult.IsValid)
            {
                return (
                    false,
                    null,
                    null,
                    validationResult.Errors.Select(x => x.ErrorMessage).ToList(),
                    false,
                    false);
            }

            var (role, roleName) = AssignmentRoleMappings.ResolveAssignmentRole(command.AssignmentRole);

            var assignment = await _dbContext.Assignments
                .Include(x => x.Member)
                .Include(x => x.Match)
                    .ThenInclude(x => x.HomeTeam)
                .Include(x => x.Match)
                    .ThenInclude(x => x.AwayTeam)
                .Include(x => x.Match)
                    .ThenInclude(x => x.Stadium)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (assignment is null)
            {
                return (false, null, null, new List<string> { "Assignment not found." }, true, false);
            }

            if (assignment.Status == AssignmentStatus.Cancelled)
            {
                return (false, null, null, new List<string> { "Cancelled assignments cannot be updated." }, false, false);
            }

            var member = await _dbContext.Members
                .FirstOrDefaultAsync(x => x.Id == command.MemberId, cancellationToken);

            if (member is null)
            {
                return (false, null, null, new List<string> { "Member not found." }, false, false);
            }

            if (!member.IsActive)
            {
                return (false, null, null, new List<string> { "Inactive members cannot be assigned." }, false, false);
            }

            var duplicateRoleExists = await _dbContext.Assignments
                .AsNoTracking()
                .Where(x =>
                    x.MatchId == assignment.MatchId &&
                    x.Id != assignment.Id &&
                    x.Status != AssignmentStatus.Cancelled)
                .Select(x => new { x.AssignmentRoleName, x.AssignmentRole })
                .ToListAsync(cancellationToken);

            var duplicateRoleName = duplicateRoleExists
                .Select(x => string.IsNullOrWhiteSpace(x.AssignmentRoleName)
                    ? AssignmentRoleMappings.GetArabicName(x.AssignmentRole)
                    : x.AssignmentRoleName)
                .Any(x => string.Equals(x, roleName, StringComparison.OrdinalIgnoreCase));

            if (duplicateRoleName)
            {
                return (false, null, null, new List<string> { $"The role '{roleName}' is already assigned for this match." }, false, false);
            }

            var duplicateMemberExists = await _dbContext.Assignments.AnyAsync(
                x =>
                    x.MatchId == assignment.MatchId &&
                    x.Id != assignment.Id &&
                    x.Status != AssignmentStatus.Cancelled &&
                    x.MemberId == command.MemberId,
                cancellationToken);

            if (duplicateMemberExists)
            {
                return (false, null, null, new List<string> { "This member is already assigned to the same match." }, false, false);
            }

            var conflict = await AssignmentConflictService.DetectConflictAsync(
                _dbContext,
                member.Id,
                member.FullName,
                assignment.MatchId,
                assignment.Match.MatchDateTime,
                assignment.Id,
                cancellationToken);

            if (conflict is not null && !command.AcknowledgeConflicts)
            {
                return (
                    false,
                    null,
                    new BulkCreateAssignmentsConflictResponse
                    {
                        Conflicts = new List<AssignmentConflictDetail> { conflict }
                    },
                    new List<string> { "Assignment conflict detected." },
                    false,
                    true);
            }

            var hasConflict = conflict is not null;
            assignment.MemberId = member.Id;
            assignment.AssignmentRole = role;
            assignment.AssignmentRoleName = roleName;
            assignment.HasConflict = hasConflict;
            assignment.ConflictMessage = hasConflict
                ? AssignmentConflictService.BuildConflictMessage(member.FullName)
                : null;
            assignment.Status = hasConflict ? AssignmentStatus.Conflict : AssignmentStatus.Confirmed;
            assignment.UpdatedAt = DateTime.UtcNow;

            AssignmentNotificationService.CreateAssignmentNotification(
                _dbContext,
                member.UserId,
                NotificationType.AssignmentUpdated,
                AssignmentNotificationService.BuildUpdatedMessage(
                    AssignmentMatchHelper.GetMatchName(assignment.Match),
                    roleName,
                    assignment.Match.MatchDateTime));

            await _dbContext.SaveChangesAsync(cancellationToken);

            assignment.Member = member;

            return (
                true,
                AssignmentResponseMapper.MapToDetails(assignment),
                null,
                new List<string>(),
                false,
                false);
        }
    }
}
