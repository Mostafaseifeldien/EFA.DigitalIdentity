using EFA.Domain.Matches;
using EFA.Domain.Members;

namespace EFA.Domain.Assignments
{
    public class Assignment
    {
        public Guid Id { get; set; }

        public Guid MatchId { get; set; }
        public Match Match { get; set; } = null!;

        public Guid MemberId { get; set; }
        public Member Member { get; set; } = null!;

        public AssignmentRole AssignmentRole { get; set; }
        public string AssignmentRoleName { get; set; } = string.Empty;
        public AssignmentStatus Status { get; set; }

        public bool HasConflict { get; set; }
        public string? ConflictMessage { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
    }
}
