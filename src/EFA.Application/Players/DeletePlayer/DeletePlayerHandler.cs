using EFA.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Players.DeletePlayer
{
    public sealed class DeletePlayerHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public DeletePlayerHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool IsSuccess, List<string> Errors, bool IsNotFound)> HandleAsync(
            DeletePlayerQuery query,
            CancellationToken cancellationToken = default)
        {
            var player = await _dbContext.Players
                .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);

            if (player is null)
            {
                return (false, new List<string> { "Player not found." }, true);
            }

            _dbContext.Players.Remove(player);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return (true, new List<string>(), false);
        }
    }
}
