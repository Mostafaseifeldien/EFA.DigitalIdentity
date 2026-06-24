using EFA.Domain.Matches;

namespace EFA.Application.Matches.GetMatchById
{
    public sealed class GetMatchByIdResponse
    {
        public Guid Id { get; set; }
        public string TournamentName { get; set; } = string.Empty;
        public string RoundName { get; set; } = string.Empty;
        public string FirstTeamName { get; set; } = string.Empty;
        public string SecondTeamName { get; set; } = string.Empty;
        public string StadiumName { get; set; } = string.Empty;
        public DateTime MatchDateTime { get; set; }
        public MatchStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
