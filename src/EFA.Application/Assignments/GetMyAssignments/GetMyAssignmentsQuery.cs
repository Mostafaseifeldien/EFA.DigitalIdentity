namespace EFA.Application.Assignments.GetMyAssignments
{
    public sealed class GetMyAssignmentsQuery
    {
        public string? UserId { get; set; }
        public Guid? MemberId { get; set; }
    }
}
