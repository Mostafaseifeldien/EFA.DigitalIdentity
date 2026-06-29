using EFA.Application.Notifications.Common;
using FluentValidation;

namespace EFA.Application.Notifications.CreateNotification
{
    public sealed class CreateNotificationCommandValidator : AbstractValidator<CreateNotificationCommand>
    {
        public CreateNotificationCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200);

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Message is required.")
                .MaximumLength(1000);

            RuleFor(x => x.TargetGroup)
                .NotEmpty().WithMessage("Target group is required.")
                .Must(x => NotificationTargetGroups.TryParse(x, out _, out _))
                .WithMessage("Invalid target group. Allowed values: RefereesOnly, AllMembers, SpecificMembers.");

            RuleFor(x => x.Priority)
                .NotEmpty().WithMessage("Priority is required.")
                .Must(x => NotificationPriorityMappings.TryParse(x, out _, out _))
                .WithMessage("Invalid priority. Allowed values: Normal, Urgent.");

            RuleFor(x => x.MemberIds)
                .NotEmpty()
                .When(x => string.Equals(
                    x.TargetGroup?.Trim(),
                    NotificationTargetGroups.SpecificMembers,
                    StringComparison.OrdinalIgnoreCase))
                .WithMessage("At least one member id is required when target group is SpecificMembers.");

            RuleFor(x => x.MemberIds)
                .Must(ids => ids is null || ids.Count == 0)
                .When(x => !string.Equals(
                    x.TargetGroup?.Trim(),
                    NotificationTargetGroups.SpecificMembers,
                    StringComparison.OrdinalIgnoreCase))
                .WithMessage("Member ids must be empty unless target group is SpecificMembers.");
        }
    }
}
