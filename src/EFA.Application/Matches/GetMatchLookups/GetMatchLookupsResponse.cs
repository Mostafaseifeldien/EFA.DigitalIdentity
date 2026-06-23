namespace EFA.Application.Matches.GetMatchLookups
{
    public sealed class MatchLookupItemResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public sealed class GetMatchLookupsResponse
    {
        public List<MatchLookupItemResponse> Tournaments { get; set; } = new();
        public List<MatchLookupItemResponse> Stadiums { get; set; } = new();
        public List<MatchLookupItemResponse> Teams { get; set; } = new();
    }
}
