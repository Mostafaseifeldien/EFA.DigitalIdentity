using FluentValidation;

namespace EFA.Application.Assignments.BulkCreateAssignments
{
    public sealed class BulkCreateAssignmentsRequestValidator : AbstractValidator<BulkCreateAssignmentsRequest>
    {
        public BulkCreateAssignmentsRequestValidator()
        {
            RuleFor(x => x.MatchId)
                .NotEmpty().WithMessage("Match is required.");

            RuleFor(x => x.Assignments)
                .NotEmpty().WithMessage("At least one assignment row is required.");

            RuleForEach(x => x.Assignments).ChildRules(item =>
            {
                item.RuleFor(x => x.MemberId)
                    .NotEmpty().WithMessage("Member is required.");

                item.RuleFor(x => x.AssignmentRole)
                    .NotEmpty().WithMessage("Assignment role is required.");
            });
        }
    }
}
