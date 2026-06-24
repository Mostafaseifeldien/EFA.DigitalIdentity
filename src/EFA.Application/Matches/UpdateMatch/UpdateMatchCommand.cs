namespace EFA.Application.Matches.UpdateMatch
{
    public sealed class UpdateMatchCommand
    {
        public string TournamentName { get; set; } = string.Empty;
        public string RoundName { get; set; } = string.Empty;
        public string FirstTeamName { get; set; } = string.Empty;
        public string SecondTeamName { get; set; } = string.Empty;
        public string StadiumName { get; set; } = string.Empty;
        public DateTime MatchDateTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
