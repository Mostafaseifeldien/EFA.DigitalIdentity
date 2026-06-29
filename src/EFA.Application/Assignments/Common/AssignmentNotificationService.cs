using EFA.Application.Common.Interfaces;
using EFA.Domain.Notifications;

namespace EFA.Application.Assignments.Common
{
    public static class AssignmentNotificationService
    {
        public static Notification CreateAssignmentNotification(
            IApplicationDbContext dbContext,
            string userId,
            NotificationType type,
            string message)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = GetTitle(type),
                Type = type,
                Message = message,
                Priority = NotificationPriority.Normal,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Notifications.Add(notification);
            return notification;
        }

        public static string BuildCreatedMessage(
            string matchName,
            string roleName,
            DateTime matchDateTime)
        {
            return $"تم تكليفك في مباراة {matchName} بدور {roleName} بتاريخ {matchDateTime:dd/MM/yyyy HH:mm}.";
        }

        public static string BuildUpdatedMessage(
            string matchName,
            string roleName,
            DateTime matchDateTime)
        {
            return $"تم تحديث تكليفك في مباراة {matchName} بدور {roleName} بتاريخ {matchDateTime:dd/MM/yyyy HH:mm}.";
        }

        public static string BuildCancelledMessage(string matchName, string roleName)
        {
            return $"تم إلغاء تكليفك في مباراة {matchName} بدور {roleName}.";
        }

        public static NotificationPushPayload MapToPushPayload(Notification notification)
        {
            return new NotificationPushPayload
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type.ToString(),
                Priority = notification.Priority.ToString(),
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }

        private static string GetTitle(NotificationType type)
        {
            return type switch
            {
                NotificationType.AssignmentCreated => "تكليف جديد",
                NotificationType.AssignmentUpdated => "تحديث تكليف",
                NotificationType.AssignmentCancelled => "إلغاء تكليف",
                _ => "إشعار"
            };
        }
    }
}
