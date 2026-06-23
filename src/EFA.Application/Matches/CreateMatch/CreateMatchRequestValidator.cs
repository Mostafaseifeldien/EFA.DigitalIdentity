using FluentValidation;

namespace EFA.Application.Matches.CreateMatch
{
    public sealed class CreateMatchRequestValidator : AbstractValidator<CreateMatchRequest>
    {
        public CreateMatchRequestValidator()
        {
            RuleFor(x => x.TournamentId)
                .NotEmpty().WithMessage("Tournament is required.");

            RuleFor(x => x.Round)
                .NotEmpty().WithMessage("Round is required.")
                .MaximumLength(100);

            RuleFor(x => x.HomeTeamName)
                .NotEmpty().WithMessage("Home team is required.")
                .MaximumLength(200);

            RuleFor(x => x.AwayTeamName)
                .NotEmpty().WithMessage("Away team is required.")
                .MaximumLength(200);

            RuleFor(x => x.StadiumId)
                .NotEmpty().WithMessage("Stadium is required.");

            RuleFor(x => x.MatchDateTime)
                .NotEmpty().WithMessage("Match date and time is required.");

            RuleFor(x => x.Notes)
                .MaximumLength(2000)
                .When(x => !string.IsNullOrWhiteSpace(x.Notes));

            RuleFor(x => x)
                .Must(x => !string.Equals(
                    x.HomeTeamName.Trim(),
                    x.AwayTeamName.Trim(),
                    StringComparison.OrdinalIgnoreCase))
                .WithMessage("Home team and away team must be different.");
        }
    }
}
