using EFA.Application.Common.Interfaces;
using EFA.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EFA.Api.Services
{
    public sealed class NotificationPushService : INotificationPushService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationPushService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task PushToUsersAsync(
            IReadOnlyDictionary<string, NotificationPushPayload> notificationsByUserId,
            CancellationToken cancellationToken = default)
        {
            foreach (var (userId, payload) in notificationsByUserId)
            {
                await _hubContext.Clients
                    .Group(userId)
                    .SendAsync("notification.received", payload, cancellationToken);
            }
        }
    }
}
