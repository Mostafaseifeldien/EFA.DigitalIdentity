using EFA.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Notifications.GetUnreadNotificationsCount
{
    public sealed class GetUnreadNotificationsCountHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetUnreadNotificationsCountHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool IsSuccess, UnreadNotificationsCountResponse? Data, List<string> Errors)> HandleAsync(
            GetUnreadNotificationsCountQuery query,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query.UserId))
            {
                return (false, null, new List<string> { "User id is required." });
            }

            var count = await _dbContext.Notifications
                .AsNoTracking()
                .CountAsync(
                    x => x.UserId == query.UserId && !x.IsRead,
                    cancellationToken);

            return (true, new UnreadNotificationsCountResponse { Count = count }, new List<string>());
        }
    }
}
