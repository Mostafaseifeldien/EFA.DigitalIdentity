namespace EFA.Domain.Matches
{
    public class Match
    {
        public Guid Id { get; set; }

        public int TournamentId { get; set; }
        public Tournament Tournament { get; set; } = null!;

        public string Round { get; set; } = string.Empty;

        public int HomeTeamId { get; set; }
        public Team HomeTeam { get; set; } = null!;

        public int AwayTeamId { get; set; }
        public Team AwayTeam { get; set; } = null!;

        public int StadiumId { get; set; }
        public Stadium Stadium { get; set; } = null!;

        public DateTime MatchDateTime { get; set; }
        public string? Notes { get; set; }

        public MatchStatus Status { get; set; }
        public bool IsPublished { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
