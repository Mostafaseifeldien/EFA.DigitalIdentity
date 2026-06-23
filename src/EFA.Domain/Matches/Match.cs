namespace EFA.Domain.Matches
{
    public class Match
    {
        public Guid Id { get; set; }

        public Guid TournamentId { get; set; }
        public Tournament Tournament { get; set; } = null!;

        public string Round { get; set; } = string.Empty;

        public Guid HomeTeamId { get; set; }
        public Team HomeTeam { get; set; } = null!;

        public Guid AwayTeamId { get; set; }
        public Team AwayTeam { get; set; } = null!;

        public Guid StadiumId { get; set; }
        public Stadium Stadium { get; set; } = null!;

        public DateTime MatchDateTime { get; set; }
        public string? Notes { get; set; }

        public MatchStatus Status { get; set; }
        public bool IsPublished { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
