namespace BadmintonCourtBooking.Models;

public class AdminDashboardViewModel
{
    public string AdminName { get; set; } = string.Empty;
    public int TotalUsers { get; set; }
    public int TotalOwners { get; set; }
    public int TotalPlayers { get; set; }
    public int LockedUsers { get; set; }
    public int TotalVenues { get; set; }
    public int ApprovedVenues { get; set; }
    public int PendingVenueCount { get; set; }
    public int RejectedVenues { get; set; }
    public int TotalCourts { get; set; }
    public int ActiveCourts { get; set; }
    public int TotalBookings { get; set; }
    public int MonthlyBookings { get; set; }
    public int PendingBookings { get; set; }
    public int ConfirmedRevenue { get; set; }
    public List<AdminPendingVenueViewModel> PendingVenues { get; set; } = new();
    public List<AdminUserOverviewViewModel> Users { get; set; } = new();
    public List<AdminVenueOverviewViewModel> Venues { get; set; } = new();
    public List<AdminBookingOverviewViewModel> RecentBookings { get; set; } = new();
}

public class AdminPendingVenueViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerPhone { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int CourtCount { get; set; }
    public string SubmittedLabel { get; set; } = string.Empty;
}

public class AdminUserOverviewViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = AppRoles.Player;
    public bool IsActive { get; set; }
    public int OwnedVenueCount { get; set; }
    public int PlayerBookingCount { get; set; }
    public string JoinedLabel { get; set; } = string.Empty;
    public string LastSignInLabel { get; set; } = string.Empty;

    public string RoleLabel => AppRoles.ToDisplayLabel(Role);

    public string RoleTone => Role switch
    {
        AppRoles.Owner => "amber",
        AppRoles.Admin => "slate",
        _ => "emerald"
    };

    public string StatusLabel => IsActive ? "Đang hoạt động" : "Đã khóa";
    public string StatusTone => IsActive ? "emerald" : "rose";

    public string ActivitySummary => Role == AppRoles.Owner
        ? $"{OwnedVenueCount} venue quản lý · {PlayerBookingCount} booking cá nhân"
        : $"{PlayerBookingCount} booking đã tạo";

    public bool CanLock => IsActive && Role != AppRoles.Admin;
    public bool CanUnlock => !IsActive && Role != AppRoles.Admin;
}

public class AdminVenueOverviewViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string OpenHours { get; set; } = string.Empty;
    public VenueStatus Status { get; set; }
    public int ActiveCourtCount { get; set; }
    public int TotalCourtCount { get; set; }
    public int PendingBookingCount { get; set; }
    public string UpdatedLabel { get; set; } = string.Empty;

    public string StatusLabel => Status switch
    {
        VenueStatus.Approved => "Đang công khai",
        VenueStatus.PendingApproval => "Chờ duyệt",
        VenueStatus.Rejected => "Đã ẩn",
        _ => "Không xác định"
    };

    public string StatusTone => Status switch
    {
        VenueStatus.Approved => "emerald",
        VenueStatus.PendingApproval => "amber",
        VenueStatus.Rejected => "rose",
        _ => "slate"
    };

    public bool CanApprove => Status != VenueStatus.Approved;
    public bool CanReject => Status != VenueStatus.Rejected;
}

public class AdminBookingOverviewViewModel
{
    public string Id { get; set; } = string.Empty;
    public string VenueName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CourtName { get; set; } = string.Empty;
    public string TimeLabel { get; set; } = string.Empty;
    public string TotalLabel { get; set; } = string.Empty;
    public BookingStatus Status { get; set; }

    public string StatusLabel => Status switch
    {
        BookingStatus.Confirmed => "Đã xác nhận",
        BookingStatus.Pending => "Chờ xử lý",
        BookingStatus.Completed => "Hoàn tất",
        BookingStatus.Cancelled => "Đã hủy",
        _ => "Không xác định"
    };

    public string StatusTone => Status switch
    {
        BookingStatus.Confirmed => "emerald",
        BookingStatus.Pending => "amber",
        BookingStatus.Completed => "sky",
        _ => "rose"
    };
}
