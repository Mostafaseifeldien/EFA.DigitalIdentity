namespace EFA.Application.Security.Common
{
    public static class SecurityDisplayHelper
    {
        public static string GetInitials(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return "?";
            }

            var parts = fullName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length >= 2)
            {
                return $"{parts[0][0]}{parts[1][0]}";
            }

            return parts[0].Length >= 2
                ? parts[0][..2]
                : parts[0];
        }

        public static string GenerateAuditReference()
        {
            return $"FR-SEC-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid():N}"[..20].ToUpperInvariant();
        }

        public static (string Status, string StatusName) GetMatchScheduleStatus(DateTime matchDateTime, bool isFinished)
        {
            if (isFinished)
            {
                return ("Finished", "منتهية");
            }

            var now = DateTime.Now;

            if (matchDateTime.Date == now.Date && matchDateTime <= now)
            {
                return ("Live", "جارية الآن");
            }

            return ("Later", "لاحقاً");
        }
    }
}
