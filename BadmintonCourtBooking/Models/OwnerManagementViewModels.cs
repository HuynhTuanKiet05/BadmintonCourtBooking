using System.ComponentModel.DataAnnotations;

namespace BadmintonCourtBooking.Models;

public class OwnerDashboardViewModel
{
    public string OwnerName { get; set; } = string.Empty;
    public string OverviewText { get; set; } = string.Empty;
    public int TotalBookings { get; set; }
    public int TodayBookings { get; set; }
    public int PendingCount { get; set; }
    public int Revenue { get; set; }
    public string ActiveCourts { get; set; } = "0/0";
    public OwnerSlotDistributionViewModel SlotDistribution { get; set; } = new();
    public List<RevenueDataPoint> RevenueData { get; set; } = new();
    public List<OwnerBookingViewModel> Bookings { get; set; } = new();
}

public class OwnerSlotDistributionViewModel
{
    public int Confirmed { get; set; }
    public int Pending { get; set; }
    public int Available { get; set; }
    public int Total => Confirmed + Pending + Available;
    public int ConfirmedPercent => Total == 0 ? 0 : (int)Math.Round(Confirmed * 100d / Total);
    public int PendingPercent => Total == 0 ? 0 : (int)Math.Round(Pending * 100d / Total);
    public int AvailablePercent => Total == 0 ? 0 : Math.Max(0, 100 - ConfirmedPercent - PendingPercent);
}

public class OwnerBookingViewModel
{
    public string Id { get; set; } = string.Empty;
    public string VenueName { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Court { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public int Total { get; set; }
    public BookingStatus Status { get; set; }
    public bool CanReview => Status == BookingStatus.Pending;
}

public class OwnerVenueManagementViewModel
{
    public string OwnerName { get; set; } = string.Empty;
    public string SelectedVenueId { get; set; } = string.Empty;
    public List<OwnerVenueViewModel> Venues { get; set; } = new();
    public bool HasVenues => Venues.Count > 0;
}

public class OwnerVenueViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string OpenTime { get; set; } = "06:00";
    public string CloseTime { get; set; } = "22:00";
    public string Description { get; set; } = string.Empty;
    public VenueStatus Status { get; set; }
    public int PriceFrom { get; set; }
    public int ActiveCourtCount { get; set; }
    public int TotalCourtCount { get; set; }
    public List<OwnerCourtViewModel> Courts { get; set; } = new();

    public string StatusLabel => Status switch
    {
        VenueStatus.Approved => "Đang công khai",
        VenueStatus.PendingApproval => "Chờ duyệt",
        VenueStatus.Rejected => "Cần cập nhật",
        _ => "Không xác định"
    };

    public string StatusTone => Status switch
    {
        VenueStatus.Approved => "emerald",
        VenueStatus.PendingApproval => "amber",
        VenueStatus.Rejected => "rose",
        _ => "slate"
    };

    public string StatusDescription => Status switch
    {
        VenueStatus.Approved => "Venue đang hiển thị trên trang public và có thể nhận booking mới.",
        VenueStatus.PendingApproval => "Venue đang chờ quản trị viên duyệt trước khi hiển thị công khai.",
        VenueStatus.Rejected => "Venue đã bị từ chối. Cập nhật lại thông tin rồi lưu để gửi duyệt lại.",
        _ => "Chưa có trạng thái phù hợp."
    };
}

public class OwnerCourtViewModel
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

public class OwnerVenueInputModel
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

    [Required(ErrorMessage = "Vui lòng nhập giờ mở cửa.")]
    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Giờ mở cửa phải theo định dạng HH:mm.")]
    public string OpenTime { get; set; } = "06:00";

    [Required(ErrorMessage = "Vui lòng nhập giờ đóng cửa.")]
    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Giờ đóng cửa phải theo định dạng HH:mm.")]
    public string CloseTime { get; set; } = "22:00";

    [Required(ErrorMessage = "Vui lòng nhập mô tả cụm sân.")]
    [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự.")]
    public string Description { get; set; } = string.Empty;
}

public class OwnerCourtInputModel
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
