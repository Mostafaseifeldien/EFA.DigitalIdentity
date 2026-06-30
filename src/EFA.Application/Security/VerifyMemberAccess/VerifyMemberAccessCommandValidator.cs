using FluentValidation;

namespace EFA.Application.Security.VerifyMemberAccess
{
    public sealed class VerifyMemberAccessCommandValidator : AbstractValidator<VerifyMemberAccessCommand>
    {
        public VerifyMemberAccessCommandValidator()
        {
            RuleFor(x => x.MatchId)
                .NotEmpty().WithMessage("Match id is required.");

            RuleFor(x => x.MemberCode)
                .NotEmpty().WithMessage("Member code is required.")
                .MaximumLength(50);

            RuleFor(x => x.GateName)
                .MaximumLength(100);

            RuleFor(x => x.ScannedByUserId)
                .NotEmpty().WithMessage("Scanned by user id is required.");
        }
    }
}
