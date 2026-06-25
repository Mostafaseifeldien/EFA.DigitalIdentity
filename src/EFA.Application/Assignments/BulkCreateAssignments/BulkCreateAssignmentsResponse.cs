using EFA.Application.Assignments.Common;
using EFA.Domain.Assignments;

namespace EFA.Application.Assignments.BulkCreateAssignments
{
    public sealed class BulkCreateAssignmentsResponse
    {
        public List<AssignmentDetailsResponse> Assignments { get; set; } = new();
    }

    public sealed class BulkCreateAssignmentsConflictResponse
    {
        public List<AssignmentConflictDetail> Conflicts { get; set; } = new();
    }
}
