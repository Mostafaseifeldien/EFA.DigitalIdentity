using EFA.Domain.Assignments;

namespace EFA.Application.Assignments.GetMyAssignments
{
    public sealed class MyAssignmentResponse
    {
        public Guid AssignmentId { get; set; }
        public Guid MatchId { get; set; }
        public string MatchName { get; set; } = string.Empty;
        public string TournamentName { get; set; } = string.Empty;
        public string StadiumName { get; set; } = string.Empty;
        public DateTime MatchDateTime { get; set; }
        public string MatchDateTimeText { get; set; } = string.Empty;
        public AssignmentRole AssignmentRole { get; set; }
        public string AssignmentRoleName { get; set; } = string.Empty;
        public AssignmentStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public bool HasConflict { get; set; }
        public string? ConflictMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
    }
}
