namespace EFA.Application.Security.Common
{
    public static class SecurityVerificationConstants
    {
        public const string ChannelName = "داخل التطبيق";

        public static class RejectionReasonCodes
        {
            public const string None = "None";
            public const string UnknownMember = "UnknownMember";
            public const string NoValidAssignment = "NoValidAssignment";
            public const string DuplicateEntry = "DuplicateEntry";
            public const string InactiveMember = "InactiveMember";
            public const string AssignmentConflict = "AssignmentConflict";
            public const string MatchNotFound = "MatchNotFound";
            public const string MatchFinished = "MatchFinished";
        }

        public static string GetRejectionReasonName(string code)
        {
            return code switch
            {
                RejectionReasonCodes.UnknownMember => "غير معروف",
                RejectionReasonCodes.NoValidAssignment => "لا يوجد تكليف ساري لهذا الشخص في هذه المباراة",
                RejectionReasonCodes.DuplicateEntry => "رمز مكرر",
                RejectionReasonCodes.InactiveMember => "العضو غير ساري",
                RejectionReasonCodes.AssignmentConflict => "التكليف في حالة تعارض",
                RejectionReasonCodes.MatchFinished => "المباراة منتهية",
                _ => "رفض الدخول"
            };
        }
    }
}
