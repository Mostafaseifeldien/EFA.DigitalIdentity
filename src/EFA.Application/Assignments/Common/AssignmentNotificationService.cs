using EFA.Application.Common.Interfaces;
using EFA.Domain.Notifications;

namespace EFA.Application.Assignments.Common
{
    public static class AssignmentNotificationService
    {
        public static void CreateAssignmentNotification(
            IApplicationDbContext dbContext,
            string userId,
            NotificationType type,
            string message)
        {
            dbContext.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
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
    }
}
