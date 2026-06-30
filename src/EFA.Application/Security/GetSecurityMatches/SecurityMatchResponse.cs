namespace EFA.Application.Security.GetSecurityMatches
{
    public sealed class SecurityMatchResponse
    {
        public Guid Id { get; set; }
        public string MatchName { get; set; } = string.Empty;
        public string StadiumName { get; set; } = string.Empty;
        public DateTime MatchDateTime { get; set; }
        public string ScheduleStatus { get; set; } = string.Empty;
        public string ScheduleStatusName { get; set; } = string.Empty;
        public string DisplaySubtitle { get; set; } = string.Empty;
    }
}
