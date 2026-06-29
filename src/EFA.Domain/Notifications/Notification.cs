namespace EFA.Domain.Notifications
{
    public class Notification
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
