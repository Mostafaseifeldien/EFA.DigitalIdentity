namespace EFA.Application.Notifications.GetAdminNotificationLog
{
    public sealed class AdminNotificationLogResponse
    {
        public string Title { get; set; } = string.Empty;
        public string TargetGroup { get; set; } = string.Empty;
        public string TargetGroupName { get; set; } = string.Empty;
        public string Channel { get; set; } = "داخل التطبيق";
        public DateTime SentAt { get; set; }
        public int RecipientsCount { get; set; }
        public string DeliveryStatus { get; set; } = "Delivered";
        public string DeliveryStatusName { get; set; } = "تم التسليم";
        public int DeliveryPercentage { get; set; } = 100;
    }
}
