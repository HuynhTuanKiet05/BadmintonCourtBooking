using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BadmintonCourtBooking.Models;

public class AdminDashboardViewModel
{
    public string AdminName { get; set; } = string.Empty;
    public int TotalUsers { get; set; }
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
    public string ContactName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
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
        AppRoles.Admin => "slate",
        _ => "emerald"
    };

    public string StatusLabel => IsActive ? "Đang hoạt động" : "Đã khóa";
    public string StatusTone => IsActive ? "emerald" : "rose";

    public string ActivitySummary => $"{PlayerBookingCount} lượt đặt đã tạo";

    public bool CanLock => IsActive && Role != AppRoles.Admin;
    public bool CanUnlock => !IsActive && Role != AppRoles.Admin;
}

public class AdminVenueOverviewViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
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
    public string ContactName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CourtName { get; set; } = string.Empty;
    public string TimeLabel { get; set; } = string.Empty;
    public string TotalLabel { get; set; } = string.Empty;
    public BookingStatus Status { get; set; }
    public bool CanReview => Status == BookingStatus.Pending;

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

public class AdminVenueManagementViewModel
{
    public string AdminName { get; set; } = string.Empty;
    public string SelectedVenueId { get; set; } = string.Empty;
    public List<AdminManagedVenueViewModel> Venues { get; set; } = new();
    public bool HasVenues => Venues.Count > 0;
}

public class AdminManagedVenueViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string OpenTime { get; set; } = "06:00";
    public string CloseTime { get; set; } = "22:00";
    public string Description { get; set; } = string.Empty;
    public VenueStatus Status { get; set; }
    public int PriceFrom { get; set; }
    public int ActiveCourtCount { get; set; }
    public int TotalCourtCount { get; set; }
    public string? ImagePath { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public List<AdminCourtViewModel> Courts { get; set; } = new();

    public string StatusLabel => Status switch
    {
        VenueStatus.Approved => "Đang công khai",
        VenueStatus.Rejected => "Đã ẩn",
        VenueStatus.PendingApproval => "Chờ xử lý",
        _ => "Không xác định"
    };

    public string StatusTone => Status switch
    {
        VenueStatus.Approved => "emerald",
        VenueStatus.Rejected => "rose",
        VenueStatus.PendingApproval => "amber",
        _ => "slate"
    };

    public string StatusDescription => Status switch
    {
        VenueStatus.Approved => "Cụm sân đang hiển thị trên trang công khai và có thể nhận lượt đặt mới.",
        VenueStatus.Rejected => "Cụm sân đang được ẩn khỏi trang công khai. Quản trị viên có thể công khai lại trên bảng điều khiển.",
        VenueStatus.PendingApproval => "Cụm sân đang ở trạng thái chờ xử lý.",
        _ => "Chưa có trạng thái phù hợp."
    };
}

public class AdminCourtViewModel
{
    public string Id { get; set; } = string.Empty;
    public string VenueId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int PricePerHour { get; set; }
    public string? Note { get; set; }
    public bool IsActive { get; set; }

    public string AvailabilityLabel => IsActive ? "Đang hoạt động" : "Tạm ngưng";
    public string AvailabilityTone => IsActive ? "emerald" : "slate";
}

public class AdminVenueInputModel
{
    public string? Id { get; set; }
    public string? SelectedVenueId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên cụm sân.")]
    [StringLength(160, ErrorMessage = "Tên cụm sân tối đa 160 ký tự.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập khu vực.")]
    [StringLength(80, ErrorMessage = "Khu vực tối đa 80 ký tự.")]
    public string District { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
    [StringLength(240, ErrorMessage = "Địa chỉ tối đa 240 ký tự.")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tên liên hệ.")]
    [StringLength(120, ErrorMessage = "Tên liên hệ tối đa 120 ký tự.")]
    public string ContactName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại liên hệ.")]
    [StringLength(30, ErrorMessage = "Số điện thoại liên hệ tối đa 30 ký tự.")]
    public string ContactPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập giờ mở cửa.")]
    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Giờ mở cửa phải theo định dạng HH:mm.")]
    public string OpenTime { get; set; } = "06:00";

    [Required(ErrorMessage = "Vui lòng nhập giờ đóng cửa.")]
    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Giờ đóng cửa phải theo định dạng HH:mm.")]
    public string CloseTime { get; set; } = "22:00";

    [Required(ErrorMessage = "Vui lòng nhập mô tả cụm sân.")]
    [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự.")]
    public string Description { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }
    public string? ExistingImagePath { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class AdminCourtInputModel
{
    public string? Id { get; set; }
    public string? SelectedVenueId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn cụm sân.")]
    public string VenueId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tên sân con.")]
    [StringLength(120, ErrorMessage = "Tên sân con tối đa 120 ký tự.")]
    public string Name { get; set; } = string.Empty;

    [Range(50_000, 500_000, ErrorMessage = "Giá thuê mỗi giờ phải từ 50.000đ đến 500.000đ.")]
    public int PricePerHour { get; set; } = 100_000;

    [StringLength(200, ErrorMessage = "Ghi chú tối đa 200 ký tự.")]
    public string? Note { get; set; }

    public bool IsActive { get; set; } = true;
}
