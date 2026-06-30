namespace EFA.Application.Security.VerifyMemberAccess
{
    public sealed class VerifyMemberAccessCommand
    {
        public Guid MatchId { get; set; }
        public string MemberCode { get; set; } = string.Empty;
        public string? GateName { get; set; }
        public string ScannedByUserId { get; set; } = string.Empty;
    }
}
