using EFA.Domain.Notifications;

namespace EFA.Application.Notifications.Common
{
    public static class NotificationPriorityMappings
    {
        public static bool TryParse(string? value, out NotificationPriority priority, out string? error)
        {
            priority = NotificationPriority.Normal;
            error = null;

            if (string.IsNullOrWhiteSpace(value))
            {
                error = "Priority is required.";
                return false;
            }

            switch (value.Trim())
            {
                case "Normal":
                    priority = NotificationPriority.Normal;
                    return true;
                case "Urgent":
                    priority = NotificationPriority.Urgent;
                    return true;
                default:
                    error = "Invalid priority. Allowed values: Normal, Urgent.";
                    return false;
            }
        }

        public static string GetName(NotificationPriority priority)
        {
            return priority.ToString();
        }
    }
}
