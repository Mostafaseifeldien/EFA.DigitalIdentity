using EFA.Domain.Assignments;

namespace EFA.Application.Assignments.Common
{
    public static class AssignmentRoleMappings
    {
        public static string GetArabicName(AssignmentRole role)
        {
            return role switch
            {
                AssignmentRole.MainReferee => "حكم رئيسي",
                AssignmentRole.LineRefereeRight => "حكم خط (يمين)",
                AssignmentRole.LineRefereeLeft => "حكم خط (يسار)",
                AssignmentRole.FourthReferee => "حكم رابع",
                AssignmentRole.VAR => "VAR",
                AssignmentRole.Observer => "مراقب",
                AssignmentRole.StadiumSecurity => "مسؤول تحقق البوابة 4",
                _ => "غير معروف"
            };
        }

        public static bool TryParseFromArabic(string? role, out AssignmentRole parsedRole, out string? error)
        {
            parsedRole = default;
            error = null;

            if (string.IsNullOrWhiteSpace(role))
            {
                error = "Assignment role is required.";
                return false;
            }

            var parsed = role.Trim() switch
            {
                "حكم رئيسي" => (AssignmentRole?)AssignmentRole.MainReferee,
                "حكم خط (يمين)" => AssignmentRole.LineRefereeRight,
                "حكم خط (يسار)" => AssignmentRole.LineRefereeLeft,
                "حكم رابع" => AssignmentRole.FourthReferee,
                "VAR" => AssignmentRole.VAR,
                "مراقب" => AssignmentRole.Observer,
                "مسؤول تحقق البوابة 4" => AssignmentRole.StadiumSecurity,
                _ => null
            };

            if (!parsed.HasValue)
            {
                error = "Invalid assignment role. Allowed values: حكم رئيسي, حكم خط (يمين), حكم خط (يسار), حكم رابع, VAR, مراقب, مسؤول تحقق البوابة 4.";
                return false;
            }

            parsedRole = parsed.Value;
            return true;
        }

        public static IReadOnlyList<AssignmentRoleLookupItem> GetAllRoles()
        {
            return Enum.GetValues<AssignmentRole>()
                .Select(role => new AssignmentRoleLookupItem
                {
                    Value = role,
                    Name = GetArabicName(role)
                })
                .ToList();
        }
    }

    public sealed class AssignmentRoleLookupItem
    {
        public AssignmentRole Value { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
