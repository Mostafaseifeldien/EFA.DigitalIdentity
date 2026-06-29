namespace EFA.Application.Notifications.GetNotificationById
{
    public sealed class GetNotificationByIdQuery
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}
