using EFA.Domain.Assignments;

namespace EFA.Application.Security.Common
{
    public static class SecurityAccessPermissions
    {
        public static string GetPermissionText(AssignmentRole role, string? assignmentRoleName)
        {
            if (!string.IsNullOrWhiteSpace(assignmentRoleName))
            {
                return assignmentRoleName switch
                {
                    "حكم رابع" or "حكم ساحة" or "حكم رئيسي" =>
                        "مسموح - منطقة الملعب وغرف الحكام",
                    "مندوب نادي الأهلي" or "مندوب نادي" =>
                        "مسموح - منطقة المدرجات والنفق",
                    _ => "مسموح - منطقة الملعب"
                };
            }

            return role switch
            {
                AssignmentRole.MainReferee or AssignmentRole.FourthReferee or
                AssignmentRole.LineRefereeLeft or AssignmentRole.LineRefereeRight or
                AssignmentRole.VAR or AssignmentRole.Observer =>
                    "مسموح - منطقة الملعب وغرف الحكام",
                AssignmentRole.StadiumSecurity =>
                    "مسموح - البوابة والمدرجات",
                _ => "مسموح - منطقة الملعب"
            };
        }
    }
}
