namespace EFA.Application.Assignments.GetAssignments
{
    public sealed class GetAssignmentsQuery
    {
        public Guid? MatchId { get; set; }
        public string? Search { get; set; }
    }
}
