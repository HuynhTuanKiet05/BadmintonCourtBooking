namespace BadmintonCourtBooking.Models;

public class AdminViewModel
{
    public int TotalOwners { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalVenues { get; set; }
    public int MonthlyBookings { get; set; }
    public List<PendingVenue> PendingVenues { get; set; } = new();
    public List<RecentUser> RecentUsers { get; set; } = new();
}

public class PendingVenue
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public int Courts { get; set; }
    public string Submitted { get; set; } = string.Empty;
}

public class RecentUser
{
    public string Name { get; set; } = string.Empty;
    public string JoinDate { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "Khách hàng" or "Chủ sân"
}
