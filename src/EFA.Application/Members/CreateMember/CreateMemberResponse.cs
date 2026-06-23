using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFA.Application.Members.CreateMember
{
    public sealed class CreateMemberResponse
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;

        public string MemberCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }

        public string Status { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
