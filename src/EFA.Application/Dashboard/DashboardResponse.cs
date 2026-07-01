namespace EFA.Application.Dashboard
{
    public sealed class DashboardMetricResponse
    {
        public decimal Value { get; set; }
        public string? Unit { get; set; }
        public decimal? ChangeValue { get; set; }
        public string ChangeLabel { get; set; } = string.Empty;
        public bool IsPositiveTrend { get; set; }
    }

    public sealed class DashboardSummaryResponse
    {
        public DashboardMetricResponse AssignmentsIssuedThisWeek { get; set; } = new();
        public DashboardMetricResponse ActualAttendanceRate { get; set; } = new();
        public DashboardMetricResponse RejectedEntryAttempts { get; set; } = new();
        public DashboardMetricResponse NotificationsSentThisMonth { get; set; } = new();
    }

    public sealed class RecentVerificationResponse
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string Initials { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public bool IsAllowed { get; set; }
        public DateTime ScannedAt { get; set; }
    }

    public sealed class AssignmentsAttendanceChartItemResponse
    {
        public DateTime Date { get; set; }
        public string DayLabel { get; set; } = string.Empty;
        public int AssignmentsCount { get; set; }
        public int AttendanceCount { get; set; }
    }

    public sealed class LatestCircularResponse
    {
        public string Title { get; set; } = string.Empty;
        public string TargetGroupName { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public string SentAtRelative { get; set; } = string.Empty;
        public int RecipientsCount { get; set; }
    }

    public sealed class DashboardResponse
    {
        public DashboardSummaryResponse Summary { get; set; } = new();
        public List<RecentVerificationResponse> RecentVerifications { get; set; } = new();
        public List<AssignmentsAttendanceChartItemResponse> AssignmentsAttendanceChart { get; set; } = new();
        public List<LatestCircularResponse> LatestCirculars { get; set; } = new();
    }
}
