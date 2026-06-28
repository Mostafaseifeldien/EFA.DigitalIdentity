using EFA.Application.Players.Common;
using FluentValidation;

namespace EFA.Application.Players.CreatePlayer
{
    public sealed class CreatePlayerCommandValidator : AbstractValidator<CreatePlayerCommand>
    {
        public CreatePlayerCommandValidator()
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

            RuleFor(x => x.BirthDate)
                .Must(x => x != default)
                .WithMessage("Birth date is required.")
                .Custom((birthDate, context) =>
                {
                    if (birthDate == default)
                    {
                        return;
                    }

                    if (!PlayerBirthDateRules.IsValid(birthDate, out var error))
                    {
                        context.AddFailure(nameof(CreatePlayerCommand.BirthDate), error!);
                    }
                });

            RuleFor(x => x.Nationality)
                .NotEmpty().WithMessage("Nationality is required.")
                .MaximumLength(100);
        }
    }
}
