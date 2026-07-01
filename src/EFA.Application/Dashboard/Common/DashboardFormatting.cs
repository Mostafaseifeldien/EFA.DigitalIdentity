namespace EFA.Application.Dashboard.Common
{
    public static class DashboardFormatting
    {
        public static string GetArabicDayInitial(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Saturday => "س",
                DayOfWeek.Sunday => "ح",
                DayOfWeek.Monday => "ن",
                DayOfWeek.Tuesday => "ث",
                DayOfWeek.Wednesday => "ر",
                DayOfWeek.Thursday => "خ",
                DayOfWeek.Friday => "ج",
                _ => "?"
            };
        }

        public static string GetRelativeSentDate(DateTime value)
        {
            var now = DateTime.Now;

            if (value.Date == now.Date)
            {
                return (now - value).TotalHours < 2 ? "منذ ساعة" : "اليوم";
            }

            if (value.Date == now.Date.AddDays(-1))
            {
                return "أمس";
            }

            var days = (now.Date - value.Date).Days;

            if (days <= 7)
            {
                return $"منذ {days} أيام";
            }

            return value.ToString("dd/MM/yyyy");
        }

        public static string GetVerificationStatusText(bool isAllowed, string rejectionReasonCode, string? rejectionReasonName)
        {
            if (isAllowed)
            {
                return "سماح بالدخول";
            }

            if (string.Equals(rejectionReasonCode, "DuplicateEntry", StringComparison.OrdinalIgnoreCase))
            {
                return "رمز مكرر — رفض الدخول";
            }

            if (string.Equals(rejectionReasonCode, "UnknownMember", StringComparison.OrdinalIgnoreCase))
            {
                return "رفض الدخول — غير معروف";
            }

            return string.IsNullOrWhiteSpace(rejectionReasonName)
                ? "رفض الدخول"
                : $"{rejectionReasonName} — رفض الدخول";
        }

        public static string GetInitials(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return "؟";
            }

            var parts = fullName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length >= 2)
            {
                return $"{parts[0][0]}{parts[1][0]}";
            }

            return parts[0].Length >= 2 ? parts[0][..2] : parts[0];
        }
    }
}
