using EFA.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Players.UpdatePlayer
{
    public sealed class UpdatePlayerHandler
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IValidator<UpdatePlayerCommand> _validator;

        public UpdatePlayerHandler(
            IApplicationDbContext dbContext,
            IValidator<UpdatePlayerCommand> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<(bool IsSuccess, PlayerResponse? Data, List<string> Errors, bool IsNotFound)> HandleAsync(
            int id,
            UpdatePlayerCommand command,
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

            var player = await _dbContext.Players
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (player is null)
            {
                return (false, null, new List<string> { "Player not found." }, true);
            }

            var fullName = command.FullName.Trim();
            var clubName = command.ClubName.Trim();
            var position = command.Position.Trim();

            var duplicatePlayer = await _dbContext.Players
                .AnyAsync(
                    x => x.Id != id && x.FullName == fullName && x.ClubName == clubName,
                    cancellationToken);

            if (duplicatePlayer)
            {
                return (false, null, new List<string>
                {
                    "A player with the same full name and club name already exists."
                }, false);
            }

            var duplicateShirtNumber = await _dbContext.Players
                .AnyAsync(
                    x => x.Id != id && x.ClubName == clubName && x.ShirtNumber == command.ShirtNumber,
                    cancellationToken);

            if (duplicateShirtNumber)
            {
                return (false, null, new List<string>
                {
                    "Shirt number must be unique within the same club."
                }, false);
            }

            player.FullName = fullName;
            player.ShirtNumber = command.ShirtNumber;
            player.ClubName = clubName;
            player.Position = position;
            player.UpdatedAt = DateTime.Now;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return (true, PlayerResponse.MapFrom(player), new List<string>(), false);
        }
    }
}
