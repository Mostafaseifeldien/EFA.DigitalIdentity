using EFA.Application.Common.Interfaces;
using EFA.Application.Notifications.Common;
using EFA.Domain.Members;
using EFA.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Notifications.GetAdminNotificationLog
{
    public sealed class GetAdminNotificationLogHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetAdminNotificationLogHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool IsSuccess, List<AdminNotificationLogResponse>? Data, List<string> Errors)> HandleAsync(
            GetAdminNotificationLogQuery query,
            CancellationToken cancellationToken = default)
        {
            if (!NotificationTargetGroups.TryParseFilter(query.TargetGroup, out var targetGroupFilter, out var targetGroupError))
            {
                return (false, null, new List<string> { targetGroupError! });
            }

            var notificationsQuery = _dbContext.Notifications
                .AsNoTracking()
                .Where(x => x.Type == NotificationType.General);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();
                notificationsQuery = notificationsQuery.Where(x => x.Title.Contains(search));
            }

            var groupedNotifications = await notificationsQuery
                .GroupBy(x => new
                {
                    x.Title,
                    x.Message,
                    x.CreatedAt,
                    x.Priority
                })
                .Select(g => new
                {
                    g.Key.Title,
                    g.Key.CreatedAt,
                    RecipientsCount = g.Count()
                })
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            var refereeRecipientsCount = await _dbContext.Members
                .AsNoTracking()
                .Where(x => x.MemberType == MemberType.Referee && x.UserId != null && x.UserId != string.Empty)
                .Select(x => x.UserId)
                .Distinct()
                .CountAsync(cancellationToken);

            var allMemberRecipientsCount = await _dbContext.Members
                .AsNoTracking()
                .Where(x => x.UserId != null && x.UserId != string.Empty)
                .Select(x => x.UserId)
                .Distinct()
                .CountAsync(cancellationToken);

            var response = groupedNotifications
                .Select(x =>
                {
                    var inferredTargetGroup = InferTargetGroup(
                        x.RecipientsCount,
                        refereeRecipientsCount,
                        allMemberRecipientsCount);

                    return new AdminNotificationLogResponse
                    {
                        Title = x.Title,
                        TargetGroup = inferredTargetGroup,
                        TargetGroupName = NotificationTargetGroups.GetArabicName(inferredTargetGroup, x.RecipientsCount),
                        Channel = "داخل التطبيق",
                        SentAt = x.CreatedAt,
                        RecipientsCount = x.RecipientsCount,
                        DeliveryStatus = "Delivered",
                        DeliveryStatusName = "تم التسليم",
                        DeliveryPercentage = 100
                    };
                })
                .Where(x =>
                    string.IsNullOrWhiteSpace(targetGroupFilter) ||
                    x.TargetGroup.Equals(targetGroupFilter, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return (true, response, new List<string>());
        }

        private static string InferTargetGroup(
            int recipientsCount,
            int refereeRecipientsCount,
            int allMemberRecipientsCount)
        {
            if (recipientsCount == refereeRecipientsCount && refereeRecipientsCount > 0)
            {
                return NotificationTargetGroups.RefereesOnly;
            }

            if (recipientsCount == allMemberRecipientsCount && allMemberRecipientsCount > 0)
            {
                return NotificationTargetGroups.AllMembers;
            }

            return NotificationTargetGroups.SpecificMembers;
        }
    }
}
