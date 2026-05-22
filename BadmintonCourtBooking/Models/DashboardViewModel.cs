namespace BadmintonCourtBooking.Models;

public class DashboardViewModel
{
    public int TodayBookings { get; set; }
    public int PendingCount { get; set; }
    public int Revenue { get; set; }
    public string ActiveCourts { get; set; } = string.Empty; // e.g. "4/4"
    public List<RevenueDataPoint> RevenueData { get; set; } = new();
    public List<OwnerBooking> RecentBookings { get; set; } = new();
}

public class RevenueDataPoint
{
    public string Day { get; set; } = string.Empty;
    public double Value { get; set; } // in millions
}
