namespace EFA.Application.Members.GetMembers
{
    public sealed class GetMembersQuery
    {
        public string? Search { get; set; }
        public string? MemberType { get; set; }
        public string? Status { get; set; }
    }
}
