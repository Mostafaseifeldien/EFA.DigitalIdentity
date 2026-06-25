using EFA.Application.Assignments.Common;
using FluentValidation;

namespace EFA.Application.Assignments.UpdateAssignment
{
    public sealed class UpdateAssignmentCommandValidator : AbstractValidator<UpdateAssignmentCommand>
    {
        public UpdateAssignmentCommandValidator()
        {
            RuleFor(x => x.MemberId)
                .NotEmpty().WithMessage("Member is required.");

            RuleFor(x => x.AssignmentRole)
                .NotEmpty().WithMessage("Assignment role is required.")
                .Must(role => AssignmentRoleMappings.TryParseFromArabic(role, out _, out _))
                .WithMessage("Invalid assignment role. Allowed values: حكم رئيسي, حكم خط (يمين), حكم خط (يسار), حكم رابع, VAR, مراقب, مسؤول تحقق البوابة 4.");
        }
    }
}
