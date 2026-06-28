namespace EFA.Application.Players.CreatePlayer
{
    public sealed class CreatePlayerCommand
    {
        public string FullName { get; set; } = string.Empty;
        public int ShirtNumber { get; set; }
        public string ClubName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateOnly BirthDate { get; set; }
        public string Nationality { get; set; } = string.Empty;
    }
}
