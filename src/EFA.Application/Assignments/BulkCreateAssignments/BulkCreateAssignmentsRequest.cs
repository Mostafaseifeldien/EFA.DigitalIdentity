namespace EFA.Application.Assignments.BulkCreateAssignments
{
    public sealed class BulkCreateAssignmentItemRequest
    {
        public Guid MemberId { get; set; }
        public string AssignmentRole { get; set; } = string.Empty;
    }

    public sealed class BulkCreateAssignmentsRequest
    {
        public Guid MatchId { get; set; }
        public List<BulkCreateAssignmentItemRequest> Assignments { get; set; } = new();
        public bool AcknowledgeConflicts { get; set; }
    }
}
