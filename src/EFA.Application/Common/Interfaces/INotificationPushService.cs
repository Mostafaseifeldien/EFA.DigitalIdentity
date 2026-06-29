namespace EFA.Application.Common.Interfaces
{
    public sealed class NotificationPushPayload
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public interface INotificationPushService
    {
        Task PushToUsersAsync(
            IReadOnlyDictionary<string, NotificationPushPayload> notificationsByUserId,
            CancellationToken cancellationToken = default);
    }
}
