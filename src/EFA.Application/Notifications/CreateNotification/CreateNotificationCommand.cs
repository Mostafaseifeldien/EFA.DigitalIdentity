namespace EFA.Application.Notifications.CreateNotification
{
    public sealed class CreateNotificationCommand
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string TargetGroup { get; set; } = string.Empty;
        public List<Guid> MemberIds { get; set; } = new();
        public string Priority { get; set; } = string.Empty;
    }
}
