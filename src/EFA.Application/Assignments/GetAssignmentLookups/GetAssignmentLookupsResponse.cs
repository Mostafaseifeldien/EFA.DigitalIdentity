namespace EFA.Application.Assignments.GetAssignmentLookups
{
    public sealed class AssignmentLookupItemResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public sealed class GetAssignmentLookupsResponse
    {
        public List<AssignmentLookupItemResponse> Matches { get; set; } = new();
        public List<AssignmentLookupItemResponse> Members { get; set; } = new();
        public List<AssignmentRoleLookupItemResponse> AssignmentRoles { get; set; } = new();
    }

    public sealed class AssignmentRoleLookupItemResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
