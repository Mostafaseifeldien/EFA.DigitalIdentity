namespace EFA.Application.Assignments.GetAssignmentLookups
{
    public sealed class MatchLookupItemResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime MatchDateTime { get; set; }
    }

    public sealed class MemberLookupItemResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public sealed class GetAssignmentLookupsResponse
    {
        public List<MatchLookupItemResponse> Matches { get; set; } = new();
        public List<MemberLookupItemResponse> Members { get; set; } = new();
        public List<AssignmentRoleLookupItemResponse> AssignmentRoles { get; set; } = new();
    }

    public sealed class AssignmentRoleLookupItemResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
