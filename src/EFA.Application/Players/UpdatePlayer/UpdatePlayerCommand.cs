namespace EFA.Application.Players.UpdatePlayer
{
    public sealed class UpdatePlayerCommand
    {
        public string FullName { get; set; } = string.Empty;
        public int ShirtNumber { get; set; }
        public string ClubName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
    }
}
