using EFA.Application.Assignments.Common;
using EFA.Application.Common.Interfaces;
using EFA.Domain.Assignments;
using EFA.Domain.Notifications;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Assignments.BulkCreateAssignments
{
    public sealed class BulkCreateAssignmentsHandler
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IValidator<BulkCreateAssignmentsRequest> _validator;
        private readonly INotificationPushService _notificationPushService;

        public BulkCreateAssignmentsHandler(
            IApplicationDbContext dbContext,
            IValidator<BulkCreateAssignmentsRequest> validator,
            INotificationPushService notificationPushService)
        {
            _dbContext = dbContext;
            _validator = validator;
            _notificationPushService = notificationPushService;
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

            var parsedRows = request.Assignments
                .Select(row =>
                {
                    var resolved = AssignmentRoleMappings.ResolveAssignmentRole(row.AssignmentRole);
                    return (row.MemberId, resolved.Role, resolved.RoleName);
                })
                .ToList();

            if (parsedRows.Select(x => x.MemberId).Distinct().Count() != parsedRows.Count)
            {
                return (false, null, null, new List<string> { "The same member cannot be assigned more than once in the same request." }, false);
            }

            if (parsedRows.Count(x =>
                    x.Role == AssignmentRole.Observer &&
                    string.Equals(x.RoleName, "مراقب", StringComparison.OrdinalIgnoreCase)) > 1)
            {
                return (false, null, null, new List<string> { "The observer role 'مراقب' cannot be duplicated in the same request." }, false);
            }

            var rowsSubjectToUniqueRoleName = parsedRows
                .Where(x =>
                    x.Role != AssignmentRole.Observer ||
                    string.Equals(x.RoleName, "مراقب", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (rowsSubjectToUniqueRoleName
                    .Select(x => x.RoleName)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count() != rowsSubjectToUniqueRoleName.Count)
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

            var existingAssignments = await _dbContext.Assignments
                .AsNoTracking()
                .Where(x =>
                    x.MatchId == request.MatchId &&
                    x.Status != AssignmentStatus.Cancelled)
                .Select(x => new
                {
                    x.MemberId,
                    x.AssignmentRole,
                    x.AssignmentRoleName
                })
                .ToListAsync(cancellationToken);

            foreach (var row in parsedRows)
            {
                var existingMemberAssignment = existingAssignments
                    .FirstOrDefault(x => x.MemberId == row.MemberId);

                if (existingMemberAssignment is null)
                {
                    continue;
                }

                var member = members.First(x => x.Id == row.MemberId);
                var existingRoleName = string.IsNullOrWhiteSpace(existingMemberAssignment.AssignmentRoleName)
                    ? AssignmentRoleMappings.GetArabicName(existingMemberAssignment.AssignmentRole)
                    : existingMemberAssignment.AssignmentRoleName;

                return (
                    false,
                    null,
                    null,
                    new List<string>
                    {
                        $"Member '{member.FullName}' is already assigned to this match with role '{existingRoleName}'."
                    },
                    false);
            }

            var duplicateRoleNames = parsedRows
                .Where(x => x.Role != AssignmentRole.Observer)
                .Select(x => x.RoleName)
                .Where(roleName => existingAssignments
                    .Where(x => x.AssignmentRole != AssignmentRole.Observer)
                    .Select(x => string.IsNullOrWhiteSpace(x.AssignmentRoleName)
                        ? AssignmentRoleMappings.GetArabicName(x.AssignmentRole)
                        : x.AssignmentRoleName)
                    .Contains(roleName, StringComparer.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (duplicateRoleNames.Count > 0)
            {
                return (false, null, null, new List<string> { $"The following roles are already assigned for this match: {string.Join(", ", duplicateRoleNames)}." }, false);
            }

            var parsedRowsToCreate = parsedRows
                .Where(row =>
                {
                    if (row.Role != AssignmentRole.Observer ||
                        !string.Equals(row.RoleName, "مراقب", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    return !existingAssignments.Any(x =>
                        x.AssignmentRole == AssignmentRole.Observer &&
                        string.Equals(x.AssignmentRoleName, "مراقب", StringComparison.OrdinalIgnoreCase));
                })
                .ToList();

            if (parsedRowsToCreate.Count == 0)
            {
                var requestedMuraqib = parsedRows.Any(row =>
                    row.Role == AssignmentRole.Observer &&
                    string.Equals(row.RoleName, "مراقب", StringComparison.OrdinalIgnoreCase));

                if (requestedMuraqib)
                {
                    var existingMuraqibAssignments = await _dbContext.Assignments
                        .AsNoTracking()
                        .Include(x => x.Member)
                        .Where(x =>
                            x.MatchId == request.MatchId &&
                            x.Status != AssignmentStatus.Cancelled &&
                            x.AssignmentRole == AssignmentRole.Observer)
                        .ToListAsync(cancellationToken);

                    var existingMuraqib = existingMuraqibAssignments
                        .FirstOrDefault(x =>
                            string.Equals(x.AssignmentRoleName, "مراقب", StringComparison.OrdinalIgnoreCase));

                    var assignedMemberName = existingMuraqib?.Member?.FullName ?? "غير معروف";

                    return (
                        false,
                        null,
                        null,
                        new List<string>
                        {
                            $"The assignment role 'مراقب' is already assigned for this match to member '{assignedMemberName}'."
                        },
                        false);
                }

                return (
                    true,
                    new BulkCreateAssignmentsResponse { Assignments = new List<AssignmentDetailsResponse>() },
                    null,
                    new List<string>(),
                    false);
            }

            var membersForConflictCheck = parsedRowsToCreate
                .Select(row => members.First(x => x.Id == row.MemberId))
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
            var createdNotifications = new List<Notification>();
            var matchName = AssignmentMatchHelper.GetMatchName(match);

            foreach (var row in parsedRowsToCreate)
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
                    AssignmentRoleName = row.RoleName,
                    Status = hasConflict ? AssignmentStatus.Conflict : AssignmentStatus.Confirmed,
                    HasConflict = hasConflict,
                    ConflictMessage = conflictMessage,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Assignments.Add(assignment);
                createdAssignments.Add(assignment);

                var notification = AssignmentNotificationService.CreateAssignmentNotification(
                    _dbContext,
                    member.UserId,
                    NotificationType.AssignmentCreated,
                    AssignmentNotificationService.BuildCreatedMessage(
                        matchName,
                        row.RoleName,
                        match.MatchDateTime));

                createdNotifications.Add(notification);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            var pushPayloads = createdNotifications
                .Where(x => !string.IsNullOrWhiteSpace(x.UserId))
                .ToDictionary(
                    x => x.UserId,
                    AssignmentNotificationService.MapToPushPayload);

            if (pushPayloads.Count > 0)
            {
                await _notificationPushService.PushToUsersAsync(pushPayloads, cancellationToken);
            }

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
