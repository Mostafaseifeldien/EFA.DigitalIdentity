namespace EFA.Application.Members.UpdateMember
{
    public sealed class UpdateMemberCommand
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MemberType { get; set; } = string.Empty;
    }
}
