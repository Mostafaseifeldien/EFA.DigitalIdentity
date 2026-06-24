using EFA.Domain.Matches;

namespace EFA.Application.Matches.Common
{
    public static class MatchStatusMappings
    {
        public static string GetArabicName(MatchStatus status)
        {
            return status switch
            {
                MatchStatus.Confirmed => "مؤكدة",
                MatchStatus.UnderPreparation => "تحت الإعداد",
                MatchStatus.Finished => "منتهية",
                _ => "غير معروف"
            };
        }

        public static bool TryParseFromArabic(string? status, out MatchStatus matchStatus, out string? error)
        {
            matchStatus = default;
            error = null;

            if (string.IsNullOrWhiteSpace(status))
            {
                error = "Status is required.";
                return false;
            }

            var parsed = status.Trim() switch
            {
                "مؤكدة" => (MatchStatus?)MatchStatus.Confirmed,
                "تحت الإعداد" => MatchStatus.UnderPreparation,
                "منتهية" => MatchStatus.Finished,
                _ => null
            };

            if (!parsed.HasValue)
            {
                error = "Invalid status value. Allowed values: مؤكدة, تحت الإعداد, منتهية.";
                return false;
            }

            matchStatus = parsed.Value;
            return true;
        }
    }
}
