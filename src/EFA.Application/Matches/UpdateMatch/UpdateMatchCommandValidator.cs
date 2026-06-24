using EFA.Application.Matches.Common;
using FluentValidation;

namespace EFA.Application.Matches.UpdateMatch
{
    public sealed class UpdateMatchCommandValidator : AbstractValidator<UpdateMatchCommand>
    {
        public UpdateMatchCommandValidator()
        {
            RuleFor(x => x.TournamentName)
                .NotEmpty().WithMessage("Tournament is required.")
                .MaximumLength(200);

            RuleFor(x => x.RoundName)
                .NotEmpty().WithMessage("Round is required.")
                .MaximumLength(100);

            RuleFor(x => x.FirstTeamName)
                .NotEmpty().WithMessage("First team is required.")
                .MaximumLength(200);

            RuleFor(x => x.SecondTeamName)
                .NotEmpty().WithMessage("Second team is required.")
                .MaximumLength(200);

            RuleFor(x => x.StadiumName)
                .NotEmpty().WithMessage("Stadium is required.")
                .MaximumLength(200);

            RuleFor(x => x.MatchDateTime)
                .NotEmpty().WithMessage("Match date and time is required.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(status => MatchStatusMappings.TryParseFromArabic(status, out _, out _))
                .WithMessage("Invalid status value. Allowed values: مؤكدة, تحت الإعداد, منتهية.");

            RuleFor(x => x.Notes)
                .MaximumLength(2000)
                .When(x => !string.IsNullOrWhiteSpace(x.Notes));

            RuleFor(x => x)
                .Must(x => !string.Equals(
                    x.FirstTeamName.Trim(),
                    x.SecondTeamName.Trim(),
                    StringComparison.OrdinalIgnoreCase))
                .WithMessage("First team and second team must be different.");
        }
    }
}
