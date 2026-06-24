using EFA.Domain.Matches;

namespace EFA.Application.Matches.CreateMatch
{
    public sealed class CreateMatchResponse
    {
        public Guid Id { get; set; }
        public string MatchName { get; set; } = string.Empty;
        public string HomeTeamName { get; set; } = string.Empty;
        public string AwayTeamName { get; set; } = string.Empty;
        public int TournamentId { get; set; }
        public string TournamentName { get; set; } = string.Empty;
        public string Round { get; set; } = string.Empty;
        public int StadiumId { get; set; }
        public string StadiumName { get; set; } = string.Empty;
        public DateTime MatchDateTime { get; set; }
        public string? Notes { get; set; }
        public MatchStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
