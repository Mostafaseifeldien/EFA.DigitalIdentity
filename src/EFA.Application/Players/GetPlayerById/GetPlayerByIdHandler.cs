using EFA.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Players.GetPlayerById
{
    public sealed class GetPlayerByIdHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetPlayerByIdHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool IsSuccess, PlayerResponse? Data, List<string> Errors, bool IsNotFound)> HandleAsync(
            GetPlayerByIdQuery query,
            CancellationToken cancellationToken = default)
        {
            var player = await _dbContext.Players
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);

            if (player is null)
            {
                return (false, null, new List<string> { "Player not found." }, true);
            }

            return (true, PlayerResponse.MapFrom(player), new List<string>(), false);
        }
    }
}
