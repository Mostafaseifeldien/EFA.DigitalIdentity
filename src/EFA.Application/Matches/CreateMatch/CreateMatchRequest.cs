namespace EFA.Application.Matches.CreateMatch
{
    public sealed class CreateMatchRequest
    {
        public Guid TournamentId { get; set; }
        public string Round { get; set; } = string.Empty;
        public string HomeTeamName { get; set; } = string.Empty;
        public string AwayTeamName { get; set; } = string.Empty;
        public Guid StadiumId { get; set; }
        public DateTime MatchDateTime { get; set; }
        public string? Notes { get; set; }
    }
}
