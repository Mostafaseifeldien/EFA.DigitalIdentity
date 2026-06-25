using EFA.Domain.Assignments;

namespace EFA.Application.Assignments.GetAssignments
{
    public sealed class GetAssignmentsResponse
    {
        public Guid Id { get; set; }
        public Guid MemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public Guid MatchId { get; set; }
        public string MatchName { get; set; } = string.Empty;
        public AssignmentRole AssignmentRole { get; set; }
        public string AssignmentRoleName { get; set; } = string.Empty;
        public AssignmentStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public bool HasConflict { get; set; }
        public string? ConflictMessage { get; set; }
        public DateTime AssignmentDate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanCancel { get; set; }
    }
}
