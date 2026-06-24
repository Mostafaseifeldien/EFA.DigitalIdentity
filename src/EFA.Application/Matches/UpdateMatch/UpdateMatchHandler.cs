using EFA.Application.Common.Interfaces;
using EFA.Application.Matches.Common;
using EFA.Domain.Matches;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Matches.UpdateMatch
{
    public sealed class UpdateMatchHandler
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IValidator<UpdateMatchCommand> _validator;

        public UpdateMatchHandler(
            IApplicationDbContext dbContext,
            IValidator<UpdateMatchCommand> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<(bool IsSuccess, UpdateMatchResponse? Data, List<string> Errors, bool IsNotFound)> HandleAsync(
            Guid id,
            UpdateMatchCommand command,
            CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);

            if (!validationResult.IsValid)
            {
                return (
                    false,
                    null,
                    validationResult.Errors.Select(x => x.ErrorMessage).ToList(),
                    false);
            }

            var match = await _dbContext.Matches
                .Include(x => x.Tournament)
                .Include(x => x.HomeTeam)
                .Include(x => x.AwayTeam)
                .Include(x => x.Stadium)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (match is null)
            {
                return (false, null, new List<string> { "Match not found." }, true);
            }

            var tournament = await _dbContext.Tournaments
                .FirstOrDefaultAsync(x => x.Name == command.TournamentName.Trim(), cancellationToken);

            if (tournament is null)
            {
                return (false, null, new List<string> { "The selected tournament was not found." }, false);
            }

            var stadium = await _dbContext.Stadiums
                .FirstOrDefaultAsync(x => x.Name == command.StadiumName.Trim(), cancellationToken);

            if (stadium is null)
            {
                return (false, null, new List<string> { "The selected stadium was not found." }, false);
            }

            if (!MatchStatusMappings.TryParseFromArabic(command.Status, out var status, out var statusError))
            {
                return (false, null, new List<string> { statusError! }, false);
            }

            var homeTeam = await GetOrCreateTeamAsync(command.FirstTeamName, cancellationToken);
            var awayTeam = await GetOrCreateTeamAsync(command.SecondTeamName, cancellationToken);

            match.TournamentId = tournament.Id;
            match.Round = command.RoundName.Trim();
            match.HomeTeamId = homeTeam.Id;
            match.AwayTeamId = awayTeam.Id;
            match.StadiumId = stadium.Id;
            match.MatchDateTime = command.MatchDateTime;
            match.Notes = string.IsNullOrWhiteSpace(command.Notes) ? null : command.Notes.Trim();
            match.Status = status;
            match.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            var response = new UpdateMatchResponse
            {
                Id = match.Id,
                TournamentName = tournament.Name,
                RoundName = match.Round,
                FirstTeamName = homeTeam.Name,
                SecondTeamName = awayTeam.Name,
                StadiumName = stadium.Name,
                MatchDateTime = match.MatchDateTime,
                Status = match.Status,
                StatusName = MatchStatusMappings.GetArabicName(match.Status),
                Notes = match.Notes,
                UpdatedAt = match.UpdatedAt
            };

            return (true, response, new List<string>(), false);
        }

        private async Task<Team> GetOrCreateTeamAsync(
            string teamName,
            CancellationToken cancellationToken)
        {
            var normalizedName = teamName.Trim();

            var existingTeam = await _dbContext.Teams
                .FirstOrDefaultAsync(x => x.Name == normalizedName, cancellationToken);

            if (existingTeam is not null)
            {
                return existingTeam;
            }

            var team = new Team
            {
                Name = normalizedName,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Teams.Add(team);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return team;
        }
    }
}
