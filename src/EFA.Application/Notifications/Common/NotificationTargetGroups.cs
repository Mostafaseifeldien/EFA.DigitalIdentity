namespace EFA.Application.Notifications.Common
{
    public static class NotificationTargetGroups
    {
        public const string RefereesOnly = "RefereesOnly";
        public const string AllMembers = "AllMembers";
        public const string SpecificMembers = "SpecificMembers";

        public static bool TryParse(string? value, out string? targetGroup, out string? error)
        {
            targetGroup = null;
            error = null;

            if (string.IsNullOrWhiteSpace(value))
            {
                error = "Target group is required.";
                return false;
            }

            var normalized = value.Trim();

            if (normalized is not (RefereesOnly or AllMembers or SpecificMembers))
            {
                error = "Invalid target group. Allowed values: RefereesOnly, AllMembers, SpecificMembers.";
                return false;
            }

            targetGroup = normalized;
            return true;
        }

        public static bool TryParseFilter(string? value, out string? targetGroup, out string? error)
        {
            targetGroup = null;
            error = null;

            if (string.IsNullOrWhiteSpace(value) || value.Trim() == "All")
            {
                return true;
            }

            return TryParse(value, out targetGroup, out error);
        }

        public static string GetArabicName(string? targetGroup, int recipientsCount)
        {
            return targetGroup switch
            {
                RefereesOnly => "الحكام فقط",
                AllMembers => "جميع الأعضاء",
                SpecificMembers => $"أعضاء محددون ({recipientsCount})",
                _ => "غير محدد"
            };
        }
    }
}
