using EFA.Application.Assignments.Common;
using EFA.Application.Common.Interfaces;
using EFA.Domain.Assignments;
using EFA.Domain.Matches;
using EFA.Domain.Notifications;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Assignments.BulkCreateAssignments
{
    public sealed class BulkCreateAssignmentsHandler
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IValidator<BulkCreateAssignmentsRequest> _validator;

        public BulkCreateAssignmentsHandler(
            IApplicationDbContext dbContext,
            IValidator<BulkCreateAssignmentsRequest> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<(
            bool IsSuccess,
            BulkCreateAssignmentsResponse? Data,
            BulkCreateAssignmentsConflictResponse? Conflicts,
            List<string> Errors,
            bool IsConflict)> HandleAsync(
            BulkCreateAssignmentsRequest request,
            CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return (
                    false,
                    null,
                    null,
                    validationResult.Errors.Select(x => x.ErrorMessage).ToList(),
                    false);
            }

            var match = await _dbContext.Matches
                .Include(x => x.HomeTeam)
                .Include(x => x.AwayTeam)
                .Include(x => x.Stadium)
                .FirstOrDefaultAsync(x => x.Id == request.MatchId, cancellationToken);

            if (match is null)
            {
                return (false, null, null, new List<string> { "Match not found." }, false);
            }

            var parsedRows = new List<(Guid MemberId, AssignmentRole Role)>();
            var errors = new List<string>();

            foreach (var row in request.Assignments)
            {
                if (!AssignmentRoleMappings.TryParseFromArabic(row.AssignmentRole, out var role, out var roleError))
                {
                    errors.Add(roleError!);
                    continue;
                }

                parsedRows.Add((row.MemberId, role));
            }

            if (errors.Count > 0)
            {
                return (false, null, null, errors, false);
            }

            if (parsedRows.Select(x => x.MemberId).Distinct().Count() != parsedRows.Count)
            {
                return (false, null, null, new List<string> { "The same member cannot be assigned more than once in the same request." }, false);
            }

            if (parsedRows.Select(x => x.Role).Distinct().Count() != parsedRows.Count)
            {
                return (false, null, null, new List<string> { "The same assignment role cannot be duplicated in the same request." }, false);
            }

            var memberIds = parsedRows.Select(x => x.MemberId).ToList();
            var members = await _dbContext.Members
                .Where(x => memberIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            if (members.Count != memberIds.Count)
            {
                return (false, null, null, new List<string> { "One or more selected members were not found." }, false);
            }

            var inactiveMembers = members.Where(x => !x.IsActive).Select(x => x.FullName).ToList();

            if (inactiveMembers.Count > 0)
            {
                return (false, null, null, new List<string> { $"Inactive members cannot be assigned: {string.Join(", ", inactiveMembers)}." }, false);
            }

            var roles = parsedRows.Select(x => x.Role).ToList();
            var existingRoles = await _dbContext.Assignments
                .Where(x =>
                    x.MatchId == request.MatchId &&
                    x.Status != AssignmentStatus.Cancelled &&
                    roles.Contains(x.AssignmentRole))
                .Select(x => x.AssignmentRole)
                .ToListAsync(cancellationToken);

            if (existingRoles.Count > 0)
            {
                var roleNames = existingRoles
                    .Select(AssignmentRoleMappings.GetArabicName)
                    .ToList();

                return (false, null, null, new List<string> { $"The following roles are already assigned for this match: {string.Join(", ", roleNames)}." }, false);
            }

            var membersForConflictCheck = members
                .Select(x => (x.Id, x.FullName))
                .ToList();

            var conflicts = await AssignmentConflictService.DetectConflictsForMembersAsync(
                _dbContext,
                match,
                membersForConflictCheck,
                cancellationToken: cancellationToken);

            if (conflicts.Count > 0 && !request.AcknowledgeConflicts)
            {
                return (
                    false,
                    null,
                    new BulkCreateAssignmentsConflictResponse { Conflicts = conflicts },
                    new List<string> { "Assignment conflicts detected." },
                    true);
            }

            var conflictMemberIds = conflicts.Select(x => x.MemberId).ToHashSet();
            var createdAssignments = new List<Assignment>();
            var matchName = AssignmentMatchHelper.GetMatchName(match);

            foreach (var row in parsedRows)
            {
                var member = members.First(x => x.Id == row.MemberId);
                var hasConflict = conflictMemberIds.Contains(row.MemberId);
                var conflictMessage = hasConflict
                    ? AssignmentConflictService.BuildConflictMessage(member.FullName)
                    : null;

                var assignment = new Assignment
                {
                    Id = Guid.NewGuid(),
                    MatchId = match.Id,
                    MemberId = member.Id,
                    AssignmentRole = row.Role,
                    Status = hasConflict ? AssignmentStatus.Conflict : AssignmentStatus.Confirmed,
                    HasConflict = hasConflict,
                    ConflictMessage = conflictMessage,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Assignments.Add(assignment);
                createdAssignments.Add(assignment);

                AssignmentNotificationService.CreateAssignmentNotification(
                    _dbContext,
                    member.UserId,
                    NotificationType.AssignmentCreated,
                    AssignmentNotificationService.BuildCreatedMessage(matchName, row.Role, match.MatchDateTime));
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            var loadedAssignments = await _dbContext.Assignments
                .AsNoTracking()
                .Include(x => x.Match)
                    .ThenInclude(x => x.HomeTeam)
                .Include(x => x.Match)
                    .ThenInclude(x => x.AwayTeam)
                .Include(x => x.Member)
                .Where(x => createdAssignments.Select(a => a.Id).Contains(x.Id))
                .ToListAsync(cancellationToken);

            return (
                true,
                new BulkCreateAssignmentsResponse
                {
                    Assignments = loadedAssignments
                        .Select(AssignmentResponseMapper.MapToDetails)
                        .ToList()
                },
                null,
                new List<string>(),
                false);
        }
    }
}
