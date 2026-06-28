namespace EFA.Domain.Players
{
    public class Player
    {
        public int Id { get; set; }
        public string PlayerCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int ShirtNumber { get; set; }
        public string ClubName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateOnly BirthDate { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
