using FluentValidation;

namespace EFA.Application.Players.UpdatePlayer
{
    public sealed class UpdatePlayerCommandValidator : AbstractValidator<UpdatePlayerCommand>
    {
        public UpdatePlayerCommandValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(200);

            RuleFor(x => x.ShirtNumber)
                .InclusiveBetween(1, 99).WithMessage("Shirt number must be between 1 and 99.");

            RuleFor(x => x.ClubName)
                .NotEmpty().WithMessage("Club name is required.")
                .MaximumLength(150);

            RuleFor(x => x.Position)
                .NotEmpty().WithMessage("Position is required.")
                .MaximumLength(100);
        }
    }
}
