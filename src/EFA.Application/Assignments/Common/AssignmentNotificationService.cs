using EFA.Application.Common.Interfaces;
using EFA.Domain.Assignments;
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
            AssignmentRole role,
            DateTime matchDateTime)
        {
            return $"تم تكليفك في مباراة {matchName} بدور {AssignmentRoleMappings.GetArabicName(role)} بتاريخ {matchDateTime:dd/MM/yyyy HH:mm}.";
        }

        public static string BuildUpdatedMessage(
            string matchName,
            AssignmentRole role,
            DateTime matchDateTime)
        {
            return $"تم تحديث تكليفك في مباراة {matchName} بدور {AssignmentRoleMappings.GetArabicName(role)} بتاريخ {matchDateTime:dd/MM/yyyy HH:mm}.";
        }

        public static string BuildCancelledMessage(string matchName, AssignmentRole role)
        {
            return $"تم إلغاء تكليفك في مباراة {matchName} بدور {AssignmentRoleMappings.GetArabicName(role)}.";
        }
    }
}
