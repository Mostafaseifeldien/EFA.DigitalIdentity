using EFA.Domain.Members;

namespace EFA.Application.Members.GetMembers
{
    public sealed class GetMembersResponse
    {
        public Guid Id { get; set; }
        public string MemberCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public MemberType MemberType { get; set; }
        public string MemberTypeName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
