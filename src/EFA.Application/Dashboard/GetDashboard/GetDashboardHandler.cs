using EFA.Application.Common.Interfaces;
using EFA.Application.Dashboard.Common;
using EFA.Application.Notifications.Common;
using EFA.Domain.Assignments;
using EFA.Domain.Members;
using EFA.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Dashboard.GetDashboard
{
    public sealed class GetDashboardHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetDashboardHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool IsSuccess, DashboardResponse? Data, List<string> Errors)> HandleAsync(
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now;
            var today = now.Date; // 1-7-2026
            var weekStart = today.AddDays(-6); // 25-6-2026
            var previousWeekStart = weekStart.AddDays(-7);//18-06-2026
            var previousWeekEnd = weekStart.AddDays(-1);//24-06-2026
            var monthStart = new DateTime(today.Year, today.Month, 1);//1-07-2026
            var previousMonthStart = monthStart.AddMonths(-1);//1-06-2026
            var previousMonthEnd = monthStart.AddDays(-1); // 30-6-2026

            var assignmentsThisWeek = await _dbContext.Assignments
                .AsNoTracking()
                .CountAsync(x => x.CreatedAt >= weekStart && x.Status ==AssignmentStatus.Confirmed, cancellationToken);

            var assignmentsPreviousWeek = await _dbContext.Assignments
                .AsNoTracking()
                .CountAsync(
                    x => x.CreatedAt >= previousWeekStart && x.CreatedAt <= previousWeekEnd && x.Status == AssignmentStatus.Confirmed,
                    cancellationToken);

            var accessLogsThisWeek = await _dbContext.MatchAccessLogs
                .AsNoTracking()
                .Where(x => x.ScannedAt >= weekStart)
                .Select(x => x.IsAllowed)
                .ToListAsync(cancellationToken);

            var accessLogsPreviousWeek = await _dbContext.MatchAccessLogs
                .AsNoTracking()
                .Where(x => x.ScannedAt >= previousWeekStart && x.ScannedAt <= previousWeekEnd)
                .Select(x => x.IsAllowed)
                .ToListAsync(cancellationToken);

            var rejectedThisWeek = accessLogsThisWeek.Count(x => !x);
            var rejectedPreviousWeek = accessLogsPreviousWeek.Count(x => !x);
            var rejectedAveragePreviousWeeks = rejectedPreviousWeek;

            var attendanceRateThisWeek = CalculateAttendanceRate(accessLogsThisWeek);
            var attendanceRatePreviousWeek = CalculateAttendanceRate(accessLogsPreviousWeek);

            var notificationsThisMonth = await CountNotificationBatchesAsync(monthStart, now, cancellationToken);
            var notificationsPreviousMonth = await CountNotificationBatchesAsync(previousMonthStart, previousMonthEnd, cancellationToken);

            var recentVerificationLogs = await _dbContext.MatchAccessLogs
                .AsNoTracking()
                .Include(x => x.Member)
                .OrderByDescending(x => x.ScannedAt)
                .Take(5)
                .ToListAsync(cancellationToken);

            var recentVerifications = recentVerificationLogs
                .Select(x => new RecentVerificationResponse
                {
                    Id = x.Id,
                    FullName = x.Member?.FullName,
                    Initials = DashboardFormatting.GetInitials(x.Member?.FullName),
                    StatusText = DashboardFormatting.GetVerificationStatusText(
                        x.IsAllowed,
                        x.RejectionReasonCode,
                        x.RejectionReasonName),
                    IsAllowed = x.IsAllowed,
                    ScannedAt = x.ScannedAt
                })
                .ToList();

            var assignmentDates = await _dbContext.Assignments
                .AsNoTracking()
                .Where(x => x.CreatedAt >= weekStart)
                .Select(x => x.CreatedAt.Date)
                .ToListAsync(cancellationToken);

            var attendanceDates = await _dbContext.MatchAccessLogs
                .AsNoTracking()
                .Where(x => x.ScannedAt >= weekStart && x.IsAllowed)
                .Select(x => x.ScannedAt.Date)
                .ToListAsync(cancellationToken);

            var chart = Enumerable.Range(0, 7)
                .Select(offset =>
                {
                    var date = weekStart.AddDays(offset);
                    return new AssignmentsAttendanceChartItemResponse
                    {
                        Date = date,
                        DayLabel = DashboardFormatting.GetArabicDayInitial(date.DayOfWeek),
                        AssignmentsCount = assignmentDates.Count(x => x == date),
                        AttendanceCount = attendanceDates.Count(x => x == date)
                    };
                })
                .ToList();

            var latestCirculars = await BuildLatestCircularsAsync(cancellationToken);

            var assignmentsChangePercent = CalculatePercentChange(assignmentsThisWeek, assignmentsPreviousWeek);
            var attendanceChange = Math.Round(attendanceRateThisWeek - attendanceRatePreviousWeek, 0);
            var rejectedChange = rejectedThisWeek - rejectedAveragePreviousWeeks;
            var notificationsChangePercent = CalculatePercentChange(notificationsThisMonth, notificationsPreviousMonth);

            var response = new DashboardResponse
            {
                Summary = new DashboardSummaryResponse
                {
                    AssignmentsIssuedThisWeek = new DashboardMetricResponse
                    {
                        Value = assignmentsThisWeek,
                        ChangeValue = assignmentsChangePercent,
                        ChangeLabel = assignmentsChangePercent.HasValue
                            ? $"{Math.Abs(assignmentsChangePercent.Value):0}% عن الأسبوع الماضي"
                            : "—",
                        IsPositiveTrend = assignmentsChangePercent >= 0
                    },
                    ActualAttendanceRate = new DashboardMetricResponse
                    {
                        Value = attendanceRateThisWeek,
                        Unit = "%",
                        ChangeValue = attendanceChange,
                        ChangeLabel = $"{Math.Abs(attendanceChange):0}%",
                        IsPositiveTrend = attendanceChange >= 0
                    },
                    RejectedEntryAttempts = new DashboardMetricResponse
                    {
                        Value = rejectedThisWeek,
                        ChangeValue = rejectedChange,
                        ChangeLabel = rejectedChange == 0
                            ? "— مستقر"
                            : $"{Math.Abs(rejectedChange)} عن المعتاد",
                        IsPositiveTrend = rejectedChange <= 0
                    },
                    NotificationsSentThisMonth = new DashboardMetricResponse
                    {
                        Value = notificationsThisMonth,
                        ChangeValue = notificationsChangePercent,
                        ChangeLabel = notificationsChangePercent is null or >= -5 and <= 5
                            ? "— مستقرة"
                            : $"{Math.Abs(notificationsChangePercent.Value):0}% عن الشهر الماضي",
                        IsPositiveTrend = notificationsChangePercent >= 0
                    }
                },
                RecentVerifications = recentVerifications,
                AssignmentsAttendanceChart = chart,
                LatestCirculars = latestCirculars
            };

            return (true, response, new List<string>());
        }

        private async Task<int> CountNotificationBatchesAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken)
        {
            var notifications = await _dbContext.Notifications
                .AsNoTracking()
                .Where(x =>
                    x.Type == NotificationType.General &&
                    x.CreatedAt >= from &&
                    x.CreatedAt <= to)
                .Select(x => new { x.Title, x.Message, x.CreatedAt, x.Priority })
                .ToListAsync(cancellationToken);

            return notifications
                .GroupBy(x => new { x.Title, x.Message, x.CreatedAt, x.Priority })
                .Count();
        }

        private async Task<List<LatestCircularResponse>> BuildLatestCircularsAsync(
            CancellationToken cancellationToken)
        {
            var groupedNotifications = await _dbContext.Notifications
                .AsNoTracking()
                .Where(x => x.Type == NotificationType.General)
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
                .Take(5)
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

            return groupedNotifications
                .Select(x =>
                {
                    var targetGroup = InferTargetGroup(
                        x.RecipientsCount,
                        refereeRecipientsCount,
                        allMemberRecipientsCount);

                    return new LatestCircularResponse
                    {
                        Title = x.Title,
                        TargetGroupName = NotificationTargetGroups.GetArabicName(targetGroup, x.RecipientsCount),
                        SentAt = x.CreatedAt,
                        SentAtRelative = DashboardFormatting.GetRelativeSentDate(x.CreatedAt),
                        RecipientsCount = x.RecipientsCount
                    };
                })
                .ToList();
        }

        private static decimal CalculateAttendanceRate(IReadOnlyList<bool> accessLogs)
        {
            if (accessLogs.Count == 0)
            {
                return 0;
            }

            var allowed = accessLogs.Count(x => x);
            return Math.Round((decimal)allowed / accessLogs.Count * 100, 0);
        }

        private static decimal? CalculatePercentChange(int current, int previous)
        {
            if (previous == 0)
            {
                return current == 0 ? 0 : 100;
            }

            return Math.Round((decimal)(current - previous) / previous * 100, 0);
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
