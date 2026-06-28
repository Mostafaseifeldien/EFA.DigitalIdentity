using EFA.Application.Common.Interfaces;
using EFA.Domain.Players;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Players.CreatePlayer
{
    public sealed class CreatePlayerHandler
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IValidator<CreatePlayerCommand> _validator;

        public CreatePlayerHandler(
            IApplicationDbContext dbContext,
            IValidator<CreatePlayerCommand> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<(bool IsSuccess, PlayerResponse? Data, List<string> Errors)> HandleAsync(
            CreatePlayerCommand command,
            CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);

            if (!validationResult.IsValid)
            {
                return (
                    false,
                    null,
                    validationResult.Errors.Select(x => x.ErrorMessage).ToList());
            }

            var fullName = command.FullName.Trim();
            var clubName = command.ClubName.Trim();
            var position = command.Position.Trim();

            var duplicatePlayer = await _dbContext.Players
                .AnyAsync(
                    x => x.FullName == fullName && x.ClubName == clubName,
                    cancellationToken);

            if (duplicatePlayer)
            {
                return (false, null, new List<string>
                {
                    "A player with the same full name and club name already exists."
                });
            }

            var duplicateShirtNumber = await _dbContext.Players
                .AnyAsync(
                    x => x.ClubName == clubName && x.ShirtNumber == command.ShirtNumber,
                    cancellationToken);

            if (duplicateShirtNumber)
            {
                return (false, null, new List<string>
                {
                    "Shirt number must be unique within the same club."
                });
            }

            var player = new Player
            {
                PlayerCode = await GeneratePlayerCodeAsync(cancellationToken),
                FullName = fullName,
                ShirtNumber = command.ShirtNumber,
                ClubName = clubName,
                Position = position,
                CreatedAt = DateTime.Now
            };

            _dbContext.Players.Add(player);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return (true, PlayerResponse.MapFrom(player), new List<string>());
        }

        private async Task<string> GeneratePlayerCodeAsync(CancellationToken cancellationToken)
        {
            var count = await _dbContext.Players.CountAsync(cancellationToken);
            return $"EFA-PLY-{count + 1:00000}";
        }
    }
}
