using EFA.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Notifications.GetNotificationById
{
    public sealed class GetNotificationByIdHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetNotificationByIdHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool IsSuccess, NotificationDetailsResponse? Data, List<string> Errors, bool IsNotFound)> HandleAsync(
            GetNotificationByIdQuery query,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query.UserId))
            {
                return (false, null, new List<string> { "User id is required." }, false);
            }

            var notification = await _dbContext.Notifications
                .FirstOrDefaultAsync(
                    x => x.Id == query.Id && x.UserId == query.UserId,
                    cancellationToken);

            if (notification is null)
            {
                return (false, null, new List<string> { "Notification not found." }, true);
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return (true, NotificationDetailsResponse.MapFrom(notification), new List<string>(), false);
        }
    }
}
