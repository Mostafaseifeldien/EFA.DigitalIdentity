using EFA.Application.Common.Interfaces;
using EFA.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EFA.Infrastructure.Services
{
    public sealed class UserRoleReader : IUserRoleReader
    {
        private readonly ApplicationDbContext _dbContext;

        public UserRoleReader(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyDictionary<string, string>> GetRoleNamesByUserIdsAsync(
            IEnumerable<string> userIds,
            CancellationToken cancellationToken = default)
        {
            var ids = userIds.Distinct().ToList();

            if (ids.Count == 0)
            {
                return new Dictionary<string, string>();
            }

            var userRoles = await _dbContext.UserRoles
                .AsNoTracking()
                .Where(x => ids.Contains(x.UserId))
                .Join(
                    _dbContext.Roles.AsNoTracking(),
                    userRole => userRole.RoleId,
                    role => role.Id,
                    (userRole, role) => new { userRole.UserId, role.Name })
                .ToListAsync(cancellationToken);

            return userRoles
                .GroupBy(x => x.UserId)
                .ToDictionary(
                    x => x.Key,
                    x => x.OrderBy(r => r.Name).First().Name ?? string.Empty);
        }
    }
}
