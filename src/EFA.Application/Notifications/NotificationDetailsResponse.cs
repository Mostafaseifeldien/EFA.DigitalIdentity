using EFA.Domain.Notifications;

namespace EFA.Application.Notifications
{
    public sealed class NotificationDetailsResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }

        public static NotificationDetailsResponse MapFrom(Notification notification)
        {
            return new NotificationDetailsResponse
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Priority = notification.Priority.ToString(),
                Type = notification.Type.ToString(),
                CreatedAt = notification.CreatedAt,
                IsRead = notification.IsRead
            };
        }
    }
}
