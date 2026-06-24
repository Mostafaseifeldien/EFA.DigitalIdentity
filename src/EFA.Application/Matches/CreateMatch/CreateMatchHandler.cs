using EFA.Application.Common.Interfaces;
using EFA.Domain.Matches;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Matches.CreateMatch
{
    public sealed class CreateMatchHandler
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IValidator<CreateMatchRequest> _validator;

        public CreateMatchHandler(
            IApplicationDbContext dbContext,
            IValidator<CreateMatchRequest> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<(bool IsSuccess, CreateMatchResponse? Data, List<string> Errors)> HandleAsync(
            CreateMatchRequest request,
            CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return (
                    false,
                    null,
                    validationResult.Errors.Select(x => x.ErrorMessage).ToList());
            }

            var tournament = await _dbContext.Tournaments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.TournamentId, cancellationToken);

            if (tournament is null)
            {
                return (false, null, new List<string> { "The selected tournament was not found." });
            }

            var stadium = await _dbContext.Stadiums
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.StadiumId, cancellationToken);

            if (stadium is null)
            {
                return (false, null, new List<string> { "The selected stadium was not found." });
            }

            var homeTeam = await GetOrCreateTeamAsync(request.HomeTeamName, cancellationToken);
            var awayTeam = await GetOrCreateTeamAsync(request.AwayTeamName, cancellationToken);

            var match = new Match
            {
                Id = Guid.NewGuid(),
                TournamentId = request.TournamentId,
                Round = request.Round.Trim(),
                HomeTeamId = homeTeam.Id,
                AwayTeamId = awayTeam.Id,
                StadiumId = request.StadiumId,
                MatchDateTime = request.MatchDateTime,
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
                Status = MatchStatus.Confirmed,
                IsPublished = true,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Matches.Add(match);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var response = new CreateMatchResponse
            {
                Id = match.Id,
                MatchName = $"{homeTeam.Name} × {awayTeam.Name}",
                HomeTeamName = homeTeam.Name,
                AwayTeamName = awayTeam.Name,
                TournamentId = tournament.Id,
                TournamentName = tournament.Name,
                Round = match.Round,
                StadiumId = stadium.Id,
                StadiumName = stadium.Name,
                MatchDateTime = match.MatchDateTime,
                Notes = match.Notes,
                Status = match.Status,
                StatusName = GetStatusArabicName(match.Status),
                IsPublished = match.IsPublished,
                CreatedAt = match.CreatedAt
            };

            return (true, response, new List<string>());
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

        private static string GetStatusArabicName(MatchStatus status)
        {
            return status switch
            {
                MatchStatus.Confirmed => "مؤكدة",
                MatchStatus.UnderPreparation => "تحت الإعداد",
                MatchStatus.Finished => "منتهية",
                _ => "غير معروف"
            };
        }
    }
}
