namespace EFA.Application.Assignments.Common
{
    public sealed class AssignmentConflictDetail
    {
        public Guid MemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public Guid ConflictingMatchId { get; set; }
        public string ConflictingMatchName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
