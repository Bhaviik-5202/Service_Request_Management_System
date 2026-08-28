namespace ServiceRequestManagementSystem.API.DTOs.Dashboard
{
    public class DashboardSummaryDto
    {
        public int TotalRequests { get; set; }

        public int PendingApprovals { get; set; }

        public int ActiveUsers { get; set; }

        public int TotalAssets { get; set; }

        public int OpenRequests { get; set; }

        public int InProgressRequests { get; set; }

        public int ResolvedRequests { get; set; }

        public int ClosedRequests { get; set; }

        public int MyAssignedRequests { get; set; }

        public int ResolvedToday { get; set; }

        public double AvgResolutionTimeHours { get; set; }

        public double SlaComplianceRate { get; set; }

        public Dictionary<string, int> PriorityBreakdown { get; set; } = new();

        public Dictionary<string, int> StatusBreakdown { get; set; } = new();
    }

    public class DepartmentReportDto
    {
        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; } = string.Empty;

        public int TotalRequests { get; set; }

        public int ResolvedRequests { get; set; }

        public double ResolutionRatePercentage { get; set; }
    }

    public class SlaReportDto
    {
        public string Priority { get; set; } = string.Empty;

        public int TargetResolutionHours { get; set; }

        public int TotalTickets { get; set; }

        public int CompliantTickets { get; set; }

        public double CompliancePercentage { get; set; }
    }

    public class TrendsReportDto
    {
        public string Month { get; set; } = string.Empty;

        public int CreatedCount { get; set; }

        public int ResolvedCount { get; set; }
    }
}
