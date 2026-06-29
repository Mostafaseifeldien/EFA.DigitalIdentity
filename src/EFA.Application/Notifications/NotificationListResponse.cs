using EFA.Domain.Notifications;

namespace EFA.Application.Notifications
{
    public sealed class NotificationListResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        public static NotificationListResponse MapFrom(Notification notification)
        {
            return new NotificationListResponse
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Priority = notification.Priority.ToString(),
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }
    }
}
