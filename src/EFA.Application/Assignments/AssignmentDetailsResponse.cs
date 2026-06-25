using EFA.Application.Assignments.Common;
using EFA.Domain.Assignments;

namespace EFA.Application.Assignments
{
    public sealed class AssignmentDetailsResponse
    {
        public Guid Id { get; set; }
        public Guid MatchId { get; set; }
        public string MatchName { get; set; } = string.Empty;
        public Guid MemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public AssignmentRole AssignmentRole { get; set; }
        public string AssignmentRoleName { get; set; } = string.Empty;
        public AssignmentStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public bool HasConflict { get; set; }
        public string? ConflictMessage { get; set; }
        public DateTime MatchDateTime { get; set; }
        public DateTime AssignmentDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public bool CanEdit { get; set; }
        public bool CanCancel { get; set; }
    }

    internal static class AssignmentResponseMapper
    {
        public static AssignmentDetailsResponse MapToDetails(Assignment assignment)
        {
            var canModify = assignment.Status != AssignmentStatus.Cancelled;

            return new AssignmentDetailsResponse
            {
                Id = assignment.Id,
                MatchId = assignment.MatchId,
                MatchName = AssignmentMatchHelper.GetMatchName(assignment.Match),
                MemberId = assignment.MemberId,
                MemberName = assignment.Member.FullName,
                AssignmentRole = assignment.AssignmentRole,
                AssignmentRoleName = AssignmentRoleMappings.GetDisplayName(assignment),
                Status = assignment.Status,
                StatusName = AssignmentStatusMappings.GetArabicName(assignment.Status),
                HasConflict = assignment.HasConflict,
                ConflictMessage = assignment.ConflictMessage,
                MatchDateTime = assignment.Match.MatchDateTime,
                AssignmentDate = assignment.CreatedAt,
                UpdatedAt = assignment.UpdatedAt,
                CancelledAt = assignment.CancelledAt,
                CanEdit = canModify,
                CanCancel = canModify
            };
        }
    }
}
