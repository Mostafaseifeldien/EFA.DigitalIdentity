using EFA.Domain.Players;

namespace EFA.Application.Players
{
    public sealed class PlayerResponse
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

        public static PlayerResponse MapFrom(Player player)
        {
            return new PlayerResponse
            {
                Id = player.Id,
                PlayerCode = player.PlayerCode,
                FullName = player.FullName,
                ShirtNumber = player.ShirtNumber,
                ClubName = player.ClubName,
                Position = player.Position,
                BirthDate = player.BirthDate,
                Nationality = player.Nationality,
                CreatedAt = player.CreatedAt,
                UpdatedAt = player.UpdatedAt
            };
        }
    }
}
