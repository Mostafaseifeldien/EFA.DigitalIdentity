namespace EFA.Application.Assignments.UpdateAssignment
{
    public sealed class UpdateAssignmentCommand
    {
        public Guid MemberId { get; set; }
        public string AssignmentRole { get; set; } = string.Empty;
        public bool AcknowledgeConflicts { get; set; }
    }
}
