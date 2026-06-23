using EFA.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Matches.GetMatchLookups
{
    public sealed class GetMatchLookupsHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetMatchLookupsHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetMatchLookupsResponse> HandleAsync(CancellationToken cancellationToken = default)
        {
            var tournaments = await _dbContext.Tournaments
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new MatchLookupItemResponse
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync(cancellationToken);

            var stadiums = await _dbContext.Stadiums
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new MatchLookupItemResponse
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync(cancellationToken);

            var teams = await _dbContext.Teams
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new MatchLookupItemResponse
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync(cancellationToken);

            return new GetMatchLookupsResponse
            {
                Tournaments = tournaments,
                Stadiums = stadiums,
                Teams = teams
            };
        }
    }
}
