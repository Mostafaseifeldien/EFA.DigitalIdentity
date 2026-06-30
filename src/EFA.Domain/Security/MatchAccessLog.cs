using EFA.Domain.Matches;
using EFA.Domain.Members;

namespace EFA.Domain.Security
{
    public class MatchAccessLog
    {
        public Guid Id { get; set; }
        public Guid MatchId { get; set; }
        public Match Match { get; set; } = null!;
        public Guid? MemberId { get; set; }
        public Member? Member { get; set; }
        public string MemberCode { get; set; } = string.Empty;
        public string ScannedByUserId { get; set; } = string.Empty;
        public string? GateName { get; set; }
        public bool IsAllowed { get; set; }
        public string RejectionReasonCode { get; set; } = string.Empty;
        public string? RejectionReasonName { get; set; }
        public Guid? AssignmentId { get; set; }
        public string? AssignmentRoleName { get; set; }
        public string? PermissionText { get; set; }
        public DateTime ScannedAt { get; set; }
        public string AuditReference { get; set; } = string.Empty;
    }
}
