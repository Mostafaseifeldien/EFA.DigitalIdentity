using EFA.Domain.Assignments;

namespace EFA.Application.Assignments.Common
{
    public static class AssignmentModificationRules
    {
        public const string ModificationNotAllowedMessage =
            "Cannot edit assignment less than 2 hours before match time or after match time.";

        public const string CancellationNotAllowedMessage =
            "Cannot cancel assignment less than 2 hours before match time or after match time.";

        public static bool CanModify(AssignmentStatus status, DateTime matchDateTime)
        {
            var now = DateTime.Now;

            return status != AssignmentStatus.Cancelled
                   && matchDateTime > now.AddHours(2);
        }

        public static bool CanModify(Assignment assignment)
        {
            return CanModify(assignment.Status, assignment.Match.MatchDateTime);
        }
    }
}
