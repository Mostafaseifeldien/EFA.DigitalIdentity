using EFA.Application.Assignments.Common;
using EFA.Application.Common.Interfaces;
using EFA.Application.Security.Common;
using EFA.Domain.Assignments;
using EFA.Domain.Matches;
using EFA.Domain.Security;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Security.VerifyMemberAccess
{
    public sealed class VerifyMemberAccessHandler
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IValidator<VerifyMemberAccessCommand> _validator;

        public VerifyMemberAccessHandler(
            IApplicationDbContext dbContext,
            IValidator<VerifyMemberAccessCommand> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<(bool IsSuccess, VerifyMemberAccessResponse? Data, List<string> Errors, bool IsNotFound)> HandleAsync(
            VerifyMemberAccessCommand command,
            CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);

            if (!validationResult.IsValid)
            {
                return (
                    false,
                    null,
                    validationResult.Errors.Select(x => x.ErrorMessage).ToList(),
                    false);
            }

            var match = await _dbContext.Matches
                .Include(x => x.HomeTeam)
                .Include(x => x.AwayTeam)
                .FirstOrDefaultAsync(x => x.Id == command.MatchId, cancellationToken);

            if (match is null)
            {
                return (false, null, new List<string> { "Match not found." }, true);
            }

            var memberCode = command.MemberCode.Trim();
            var matchName = AssignmentMatchHelper.GetMatchName(match);
            var scannedAt = DateTime.Now;
            var auditReference = SecurityDisplayHelper.GenerateAuditReference();

            var member = await _dbContext.Members
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MemberCode == memberCode, cancellationToken);

            if (member is null)
            {
                var unknownLog = await CreateLogAsync(
                    match.Id,
                    null,
                    memberCode,
                    command,
                    isAllowed: false,
                    rejectionCode: SecurityVerificationConstants.RejectionReasonCodes.UnknownMember,
                    assignmentId: null,
                    assignmentRoleName: null,
                    permissionText: null,
                    scannedAt,
                    auditReference,
                    cancellationToken);

                var unknownResponse = BuildResponse(
                    unknownLog,
                    matchName,
                    member: null,
                    assignmentRoleName: null,
                    permissionText: null,
                    isDuplicate: false,
                    previousAllowedScanAt: null,
                    rejectionDetail: null,
                    summary: await GetSummaryAsync(match.Id, cancellationToken));

                return (true, unknownResponse, new List<string>(), false);
            }

            var previousAllowedScan = await _dbContext.MatchAccessLogs
                .AsNoTracking()
                .Where(x =>
                    x.MatchId == match.Id &&
                    x.MemberId == member.Id &&
                    x.IsAllowed)
                .OrderByDescending(x => x.ScannedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (previousAllowedScan is not null)
            {
                var duplicateLog = await CreateLogAsync(
                    match.Id,
                    member.Id,
                    memberCode,
                    command,
                    isAllowed: false,
                    rejectionCode: SecurityVerificationConstants.RejectionReasonCodes.DuplicateEntry,
                    assignmentId: previousAllowedScan.AssignmentId,
                    assignmentRoleName: previousAllowedScan.AssignmentRoleName,
                    permissionText: null,
                    scannedAt,
                    auditReference,
                    cancellationToken);

                var duplicateDetail =
                    $"تم استخدام هذا الرمز مسبقاً للدخول الساعة {previousAllowedScan.ScannedAt:h:mm tt} — قد يكون رمزاً منتهي الصلاحية أو محاولة استخدام مكرر للرمز";

                var duplicateResponse = BuildResponse(
                    duplicateLog,
                    matchName,
                    member,
                    previousAllowedScan.AssignmentRoleName,
                    permissionText: null,
                    isDuplicate: true,
                    previousAllowedScanAt: previousAllowedScan.ScannedAt,
                    rejectionDetail: duplicateDetail,
                    summary: await GetSummaryAsync(match.Id, cancellationToken));

                return (true, duplicateResponse, new List<string>(), false);
            }

            if (match.Status == MatchStatus.Finished)
            {
                var finishedLog = await CreateLogAsync(
                    match.Id,
                    member.Id,
                    memberCode,
                    command,
                    isAllowed: false,
                    rejectionCode: SecurityVerificationConstants.RejectionReasonCodes.MatchFinished,
                    assignmentId: null,
                    assignmentRoleName: null,
                    permissionText: null,
                    scannedAt,
                    auditReference,
                    cancellationToken);

                var finishedResponse = BuildResponse(
                    finishedLog,
                    matchName,
                    member,
                    assignmentRoleName: null,
                    permissionText: null,
                    isDuplicate: false,
                    previousAllowedScanAt: null,
                    rejectionDetail: null,
                    summary: await GetSummaryAsync(match.Id, cancellationToken));

                return (true, finishedResponse, new List<string>(), false);
            }

            if (!member.IsActive)
            {
                var inactiveLog = await CreateLogAsync(
                    match.Id,
                    member.Id,
                    memberCode,
                    command,
                    isAllowed: false,
                    rejectionCode: SecurityVerificationConstants.RejectionReasonCodes.InactiveMember,
                    assignmentId: null,
                    assignmentRoleName: null,
                    permissionText: null,
                    scannedAt,
                    auditReference,
                    cancellationToken);

                var inactiveResponse = BuildResponse(
                    inactiveLog,
                    matchName,
                    member,
                    assignmentRoleName: null,
                    permissionText: null,
                    isDuplicate: false,
                    previousAllowedScanAt: null,
                    rejectionDetail: null,
                    summary: await GetSummaryAsync(match.Id, cancellationToken));

                return (true, inactiveResponse, new List<string>(), false);
            }

            var assignment = await _dbContext.Assignments
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x =>
                        x.MatchId == match.Id &&
                        x.MemberId == member.Id &&
                        x.Status != AssignmentStatus.Cancelled,
                    cancellationToken);

            if (assignment is null)
            {
                var noAssignmentLog = await CreateLogAsync(
                    match.Id,
                    member.Id,
                    memberCode,
                    command,
                    isAllowed: false,
                    rejectionCode: SecurityVerificationConstants.RejectionReasonCodes.NoValidAssignment,
                    assignmentId: null,
                    assignmentRoleName: null,
                    permissionText: null,
                    scannedAt,
                    auditReference,
                    cancellationToken);

                var noAssignmentResponse = BuildResponse(
                    noAssignmentLog,
                    matchName,
                    member,
                    assignmentRoleName: null,
                    permissionText: null,
                    isDuplicate: false,
                    previousAllowedScanAt: null,
                    rejectionDetail: null,
                    summary: await GetSummaryAsync(match.Id, cancellationToken));

                return (true, noAssignmentResponse, new List<string>(), false);
            }

            if (assignment.Status == AssignmentStatus.Conflict)
            {
                var roleName = AssignmentRoleMappings.GetDisplayName(assignment);

                var conflictLog = await CreateLogAsync(
                    match.Id,
                    member.Id,
                    memberCode,
                    command,
                    isAllowed: false,
                    rejectionCode: SecurityVerificationConstants.RejectionReasonCodes.AssignmentConflict,
                    assignmentId: assignment.Id,
                    assignmentRoleName: roleName,
                    permissionText: null,
                    scannedAt,
                    auditReference,
                    cancellationToken);

                var conflictResponse = BuildResponse(
                    conflictLog,
                    matchName,
                    member,
                    roleName,
                    permissionText: null,
                    isDuplicate: false,
                    previousAllowedScanAt: null,
                    rejectionDetail: null,
                    summary: await GetSummaryAsync(match.Id, cancellationToken));

                return (true, conflictResponse, new List<string>(), false);
            }

            var assignmentRoleName = AssignmentRoleMappings.GetDisplayName(assignment);
            var permissionText = SecurityAccessPermissions.GetPermissionText(
                assignment.AssignmentRole,
                assignmentRoleName);

            var allowedLog = await CreateLogAsync(
                match.Id,
                member.Id,
                memberCode,
                command,
                isAllowed: true,
                rejectionCode: SecurityVerificationConstants.RejectionReasonCodes.None,
                assignmentId: assignment.Id,
                assignmentRoleName: assignmentRoleName,
                permissionText: permissionText,
                scannedAt,
                auditReference,
                cancellationToken);

            var allowedResponse = BuildResponse(
                allowedLog,
                matchName,
                member,
                assignmentRoleName,
                permissionText,
                isDuplicate: false,
                previousAllowedScanAt: null,
                rejectionDetail: null,
                summary: await GetSummaryAsync(match.Id, cancellationToken));

            return (true, allowedResponse, new List<string>(), false);
        }

        private async Task<MatchAccessLog> CreateLogAsync(
            Guid matchId,
            Guid? memberId,
            string memberCode,
            VerifyMemberAccessCommand command,
            bool isAllowed,
            string rejectionCode,
            Guid? assignmentId,
            string? assignmentRoleName,
            string? permissionText,
            DateTime scannedAt,
            string auditReference,
            CancellationToken cancellationToken)
        {
            var log = new MatchAccessLog
            {
                Id = Guid.NewGuid(),
                MatchId = matchId,
                MemberId = memberId,
                MemberCode = memberCode,
                ScannedByUserId = command.ScannedByUserId,
                GateName = string.IsNullOrWhiteSpace(command.GateName)
                    ? null
                    : command.GateName.Trim(),
                IsAllowed = isAllowed,
                RejectionReasonCode = rejectionCode,
                RejectionReasonName = isAllowed
                    ? null
                    : SecurityVerificationConstants.GetRejectionReasonName(rejectionCode),
                AssignmentId = assignmentId,
                AssignmentRoleName = assignmentRoleName,
                PermissionText = permissionText,
                ScannedAt = scannedAt,
                AuditReference = auditReference
            };

            _dbContext.MatchAccessLogs.Add(log);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return log;
        }

        private static VerifyMemberAccessResponse BuildResponse(
            MatchAccessLog log,
            string matchName,
            Domain.Members.Member? member,
            string? assignmentRoleName,
            string? permissionText,
            bool isDuplicate,
            DateTime? previousAllowedScanAt,
            string? rejectionDetail,
            ScanSummaryResponse summary)
        {
            return new VerifyMemberAccessResponse
            {
                LogId = log.Id,
                IsAllowed = log.IsAllowed,
                Status = log.IsAllowed ? "Allowed" : "Denied",
                StatusName = log.IsAllowed ? "السماح بالدخول" : "رفض الدخول",
                RejectionReasonCode = log.IsAllowed ? null : log.RejectionReasonCode,
                RejectionReasonName = log.RejectionReasonName,
                RejectionDetail = rejectionDetail ?? log.RejectionReasonName,
                IsDuplicateEntry = isDuplicate,
                PreviousAllowedScanAt = previousAllowedScanAt,
                MemberId = member?.Id,
                MemberCode = member?.MemberCode ?? log.MemberCode,
                FullName = member?.FullName,
                PhotoUrl = member?.PhotoUrl,
                Initials = SecurityDisplayHelper.GetInitials(member?.FullName),
                AssignmentRoleName = assignmentRoleName,
                MatchName = matchName,
                PermissionText = permissionText,
                ScannedAt = log.ScannedAt,
                AuditReference = log.AuditReference,
                Summary = summary
            };
        }

        private async Task<ScanSummaryResponse> GetSummaryAsync(
            Guid matchId,
            CancellationToken cancellationToken)
        {
            var logs = await _dbContext.MatchAccessLogs
                .AsNoTracking()
                .Where(x => x.MatchId == matchId)
                .Select(x => x.IsAllowed)
                .ToListAsync(cancellationToken);

            return new ScanSummaryResponse
            {
                TotalScans = logs.Count,
                AllowedCount = logs.Count(x => x),
                RejectedCount = logs.Count(x => !x)
            };
        }
    }
}
