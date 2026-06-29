using EFA.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Notifications.GetNotifications
{
    public sealed class GetNotificationsHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetNotificationsHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool IsSuccess, List<NotificationListResponse>? Data, List<string> Errors)> HandleAsync(
            GetNotificationsQuery query,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query.UserId))
            {
                return (false, null, new List<string> { "User id is required." });
            }

            var notifications = await _dbContext.Notifications
                .AsNoTracking()
                .Where(x => x.UserId == query.UserId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            var response = notifications
                .Select(NotificationListResponse.MapFrom)
                .ToList();

            return (true, response, new List<string>());
        }
    }
}
