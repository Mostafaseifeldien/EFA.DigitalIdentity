namespace EFA.Application.Security.GetMatchAccessLog
{
    public sealed class MatchAccessLogEntryResponse
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string MemberCode { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string DisplayText { get; set; } = string.Empty;
        public DateTime ScannedAt { get; set; }
        public bool IsAllowed { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? RejectionReasonName { get; set; }
    }

    public sealed class MatchAccessLogResponse
    {
        public Guid MatchId { get; set; }
        public string MatchName { get; set; } = string.Empty;
        public int TotalScans { get; set; }
        public int AllowedCount { get; set; }
        public int RejectedCount { get; set; }
        public List<MatchAccessLogEntryResponse> Entries { get; set; } = new();
    }
}
