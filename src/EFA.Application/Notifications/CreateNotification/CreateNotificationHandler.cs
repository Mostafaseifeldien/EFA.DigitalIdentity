using EFA.Application.Common.Interfaces;
using EFA.Application.Notifications.Common;
using EFA.Domain.Members;
using EFA.Domain.Notifications;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Notifications.CreateNotification
{
    public sealed class CreateNotificationHandler
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IValidator<CreateNotificationCommand> _validator;
        private readonly INotificationPushService _notificationPushService;

        public CreateNotificationHandler(
            IApplicationDbContext dbContext,
            IValidator<CreateNotificationCommand> validator,
            INotificationPushService notificationPushService)
        {
            _dbContext = dbContext;
            _validator = validator;
            _notificationPushService = notificationPushService;
        }

        public async Task<(bool IsSuccess, CreateNotificationResponse? Data, List<string> Errors)> HandleAsync(
            CreateNotificationCommand command,
            CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);

            if (!validationResult.IsValid)
            {
                return (
                    false,
                    null,
                    validationResult.Errors.Select(x => x.ErrorMessage).ToList());
            }

            if (!NotificationTargetGroups.TryParse(command.TargetGroup, out var targetGroup, out var targetGroupError))
            {
                return (false, null, new List<string> { targetGroupError! });
            }

            if (!NotificationPriorityMappings.TryParse(command.Priority, out var priority, out var priorityError))
            {
                return (false, null, new List<string> { priorityError! });
            }

            var recipientsResult = await ResolveRecipientsAsync(
                targetGroup!,
                command.MemberIds,
                cancellationToken);

            if (!recipientsResult.IsSuccess)
            {
                return (false, null, recipientsResult.Errors);
            }

            var recipients = recipientsResult.Recipients;

            if (recipients.Count == 0)
            {
                return (false, null, new List<string> { "No recipients were found for the selected target group." });
            }

            var title = command.Title.Trim();
            var message = command.Message.Trim();
            var createdAt = DateTime.Now;
            var notifications = new List<Notification>();
            var pushPayloads = new Dictionary<string, NotificationPushPayload>();

            foreach (var recipient in recipients)
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = recipient.UserId,
                    Title = title,
                    Type = NotificationType.General,
                    Message = message,
                    Priority = priority,
                    IsRead = false,
                    CreatedAt = createdAt
                };

                notifications.Add(notification);
                pushPayloads[recipient.UserId] = new NotificationPushPayload
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Message = notification.Message,
                    Priority = NotificationPriorityMappings.GetName(notification.Priority),
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt
                };
            }

            _dbContext.Notifications.AddRange(notifications);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _notificationPushService.PushToUsersAsync(pushPayloads, cancellationToken);

            return (true, new CreateNotificationResponse
            {
                RecipientCount = recipients.Count,
                NotificationsCreated = notifications.Count
            }, new List<string>());
        }

        private async Task<(bool IsSuccess, List<RecipientMember> Recipients, List<string> Errors)> ResolveRecipientsAsync(
            string targetGroup,
            IReadOnlyList<Guid> memberIds,
            CancellationToken cancellationToken)
        {
            IQueryable<Member> membersQuery = _dbContext.Members.AsNoTracking();

            switch (targetGroup)
            {
                case NotificationTargetGroups.RefereesOnly:
                    membersQuery = membersQuery.Where(x => x.MemberType == MemberType.Referee);
                    break;

                case NotificationTargetGroups.AllMembers:
                    break;

                case NotificationTargetGroups.SpecificMembers:
                    var distinctMemberIds = memberIds.Distinct().ToList();

                    membersQuery = membersQuery.Where(x => distinctMemberIds.Contains(x.Id));

                    var foundMembers = await membersQuery
                        .Select(x => x.Id)
                        .ToListAsync(cancellationToken);

                    var missingMemberIds = distinctMemberIds
                        .Except(foundMembers)
                        .ToList();

                    if (missingMemberIds.Count > 0)
                    {
                        return (
                            false,
                            new List<RecipientMember>(),
                            new List<string> { "One or more selected members were not found." });
                    }

                    break;

                default:
                    return (
                        false,
                        new List<RecipientMember>(),
                        new List<string> { "Invalid target group." });
            }

            var members = await membersQuery
                .Select(x => new RecipientMember(x.Id, x.UserId))
                .ToListAsync(cancellationToken);

            var recipients = members
                .Where(x => !string.IsNullOrWhiteSpace(x.UserId))
                .GroupBy(x => x.UserId, StringComparer.Ordinal)
                .Select(x => x.First())
                .ToList();

            return (true, recipients, new List<string>());
        }

        private sealed record RecipientMember(Guid MemberId, string UserId);
    }
}
