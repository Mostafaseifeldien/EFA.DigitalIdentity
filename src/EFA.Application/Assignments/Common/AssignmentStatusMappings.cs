using EFA.Domain.Assignments;

namespace EFA.Application.Assignments.Common
{
    public static class AssignmentStatusMappings
    {
        public static string GetArabicName(AssignmentStatus status)
        {
            return status switch
            {
                AssignmentStatus.Confirmed => "مؤكد",
                AssignmentStatus.Conflict => "تعارض",
                AssignmentStatus.Cancelled => "ملغي",
                _ => "غير معروف"
            };
        }
    }
}
