namespace EFA.Application.Security.VerifyMemberAccess
{
    public sealed class ScanSummaryResponse
    {
        public int TotalScans { get; set; }
        public int AllowedCount { get; set; }
        public int RejectedCount { get; set; }
    }

    public sealed class VerifyMemberAccessResponse
    {
        public Guid LogId { get; set; }
        public bool IsAllowed { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string? RejectionReasonCode { get; set; }
        public string? RejectionReasonName { get; set; }
        public string? RejectionDetail { get; set; }
        public bool IsDuplicateEntry { get; set; }
        public DateTime? PreviousAllowedScanAt { get; set; }
        public Guid? MemberId { get; set; }
        public string? MemberCode { get; set; }
        public string? FullName { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Initials { get; set; }
        public string? AssignmentRoleName { get; set; }
        public string? MatchName { get; set; }
        public string? PermissionText { get; set; }
        public DateTime ScannedAt { get; set; }
        public string AuditReference { get; set; } = string.Empty;
        public ScanSummaryResponse Summary { get; set; } = new();
    }
}
