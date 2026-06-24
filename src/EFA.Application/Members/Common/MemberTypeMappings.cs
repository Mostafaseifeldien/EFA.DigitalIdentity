using EFA.Domain.Members;

namespace EFA.Application.Members.Common
{
    public static class MemberTypeMappings
    {
        public static string GetArabicName(MemberType memberType)
        {
            return memberType switch
            {
                MemberType.Referee => "حكام",
                MemberType.Player => "لاعبون",
                MemberType.ClubDelegate => "مندوبي أندية",
                MemberType.SecurityOfficer => "أمن",
                MemberType.Staff => "موظفون",
                MemberType.Member => "عضو",
                _ => "عضو"
            };
        }

        public static bool TryParseFromArabic(string? memberType, out MemberType parsedMemberType, out string? error)
        {
            parsedMemberType = default;
            error = null;

            if (string.IsNullOrWhiteSpace(memberType))
            {
                error = "Member type is required.";
                return false;
            }

            var parsed = memberType.Trim() switch
            {
                "حكام" => (MemberType?)MemberType.Referee,
                "لاعبون" => MemberType.Player,
                "مندوبي أندية" => MemberType.ClubDelegate,
                "أمن" => MemberType.SecurityOfficer,
                "موظفون" => MemberType.Staff,
                "عضو" => MemberType.Member,
                _ => null
            };

            if (!parsed.HasValue)
            {
                error = "Invalid memberType value. Allowed values: حكام, لاعبون, مندوبي أندية, أمن, موظفون, عضو.";
                return false;
            }

            parsedMemberType = parsed.Value;
            return true;
        }
    }
}
