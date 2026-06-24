using EFA.Application.Members.Common;
using FluentValidation;

namespace EFA.Application.Members.UpdateMember
{
    public sealed class UpdateMemberCommandValidator : AbstractValidator<UpdateMemberCommand>
    {
        public UpdateMemberCommandValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(20);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MaximumLength(150);

            RuleFor(x => x.MemberType)
                .NotEmpty().WithMessage("Member type is required.")
                .Must(memberType => MemberTypeMappings.TryParseFromArabic(memberType, out _, out _))
                .WithMessage("Invalid memberType value. Allowed values: حكام, لاعبون, مندوبي أندية, أمن, موظفون, عضو.");
        }
    }
}
