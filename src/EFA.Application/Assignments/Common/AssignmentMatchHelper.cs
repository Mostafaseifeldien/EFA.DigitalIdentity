using EFA.Domain.Matches;

namespace EFA.Application.Assignments.Common
{
    public static class AssignmentMatchHelper
    {
        public static string GetMatchName(Match match)
        {
            return $"{match.HomeTeam.Name} × {match.AwayTeam.Name}";
        }

        public static string GetMatchDisplayName(Match match)
        {
            return $"{match.HomeTeam.Name} × {match.AwayTeam.Name} — {match.MatchDateTime:dd/MM/yyyy} — {match.Stadium.Name}";
        }
    }
}
