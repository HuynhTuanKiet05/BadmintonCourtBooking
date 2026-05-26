using BadmintonCourtBooking.Data;
using BadmintonCourtBooking.Data.Entities;
using BadmintonCourtBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Services;

public class AdminDashboardService(ApplicationDbContext context, ICurrentUserService currentUserService) : IAdminDashboardService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<AdminDashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var users = await _context.Users
            .AsNoTracking()
            .Where(user => user.Role != AppRoles.Admin)
            .OrderByDescending(user => user.JoinedAt)
            .ToListAsync(cancellationToken);

        var venues = await _context.Venues
            .AsNoTracking()
            .AsSplitQuery()
            .Include(venue => venue.Courts)
            .ToListAsync(cancellationToken);

        var bookings = await _context.Bookings
            .AsNoTracking()
            .AsSplitQuery()
            .Include(booking => booking.Court)
                .ThenInclude(court => court!.Venue)
            .OrderByDescending(booking => booking.StartAt)
            .ToListAsync(cancellationToken);

        var bookingSummaries = bookings
            .Select(booking => new
            {
                Booking = booking,
                EffectiveStatus = ResolveBookingStatus(booking),
                VenueId = booking.Court?.VenueId ?? string.Empty
            })
            .ToList();

        var bookingsByPlayer = bookings
            .Where(booking => !string.IsNullOrWhiteSpace(booking.PlayerUserId))
            .GroupBy(booking => booking.PlayerUserId!)
            .ToDictionary(group => group.Key, group => group.Count());

        var venuesByOwner = venues
            .Where(venue => !string.IsNullOrWhiteSpace(venue.OwnerUserId))
            .GroupBy(venue => venue.OwnerUserId!)
            .ToDictionary(group => group.Key, group => group.Count());

        var pendingBookingsByVenue = bookingSummaries
            .Where(item => item.EffectiveStatus == BookingStatus.Pending && !string.IsNullOrWhiteSpace(item.VenueId))
            .GroupBy(item => item.VenueId)
            .ToDictionary(group => group.Key, group => group.Count());

        var today = DateTime.Today;

        return new AdminDashboardViewModel
        {
            AdminName = _currentUserService.User?.FullName ?? "Quản trị viên CourtBook",
            TotalUsers = users.Count,
            TotalOwners = users.Count(user => user.Role == AppRoles.Owner),
            TotalPlayers = users.Count(user => user.Role == AppRoles.Player),
            LockedUsers = users.Count(user => !user.IsActive),
            TotalVenues = venues.Count,
            ApprovedVenues = venues.Count(venue => venue.Status == VenueStatus.Approved),
            PendingVenueCount = venues.Count(venue => venue.Status == VenueStatus.PendingApproval),
            RejectedVenues = venues.Count(venue => venue.Status == VenueStatus.Rejected),
            TotalCourts = venues.Sum(venue => venue.Courts.Count),
            ActiveCourts = venues.Sum(venue => venue.Courts.Count(court => court.IsActive)),
            TotalBookings = bookings.Count,
            MonthlyBookings = bookingSummaries.Count(item =>
                item.EffectiveStatus != BookingStatus.Cancelled
                && item.Booking.StartAt.Month == today.Month
                && item.Booking.StartAt.Year == today.Year),
            PendingBookings = bookingSummaries.Count(item => item.EffectiveStatus == BookingStatus.Pending),
            ConfirmedRevenue = bookingSummaries
                .Where(item => item.EffectiveStatus is BookingStatus.Confirmed or BookingStatus.Completed)
                .Sum(item => item.Booking.TotalPrice),
            PendingVenues = venues
                .Where(venue => venue.Status == VenueStatus.PendingApproval)
                .OrderByDescending(venue => venue.CreatedAt)
                .Select(venue => new AdminPendingVenueViewModel
                {
                    Id = venue.Id,
                    Name = venue.Name,
                    OwnerName = venue.OwnerName,
                    OwnerPhone = venue.OwnerPhone,
                    District = venue.District,
                    Address = venue.Address,
                    CourtCount = venue.Courts.Count,
                    SubmittedLabel = PresentationFormatter.FormatJoinDate(venue.CreatedAt)
                })
                .ToList(),
            Users = users
                .Select(user => new AdminUserOverviewViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    OwnedVenueCount = venuesByOwner.GetValueOrDefault(user.Id),
                    PlayerBookingCount = bookingsByPlayer.GetValueOrDefault(user.Id),
                    JoinedLabel = PresentationFormatter.FormatJoinDate(user.JoinedAt),
                    LastSignInLabel = user.LastSignInAt.HasValue
                        ? user.LastSignInAt.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
                        : "Chưa đăng nhập"
                })
                .ToList(),
            Venues = venues
                .OrderBy(venue => GetVenueSortOrder(venue.Status))
                .ThenByDescending(venue => venue.UpdatedAt)
                .Select(venue => new AdminVenueOverviewViewModel
                {
                    Id = venue.Id,
                    Name = venue.Name,
                    OwnerName = venue.OwnerName,
                    District = venue.District,
                    Address = venue.Address,
                    OpenHours = venue.OpenHours,
                    Status = venue.Status,
                    ActiveCourtCount = venue.Courts.Count(court => court.IsActive),
                    TotalCourtCount = venue.Courts.Count,
                    PendingBookingCount = pendingBookingsByVenue.GetValueOrDefault(venue.Id),
                    UpdatedLabel = PresentationFormatter.FormatJoinDate(venue.UpdatedAt)
                })
                .ToList(),
            RecentBookings = bookingSummaries
                .Take(12)
                .Select(item => new AdminBookingOverviewViewModel
                {
                    Id = item.Booking.Id,
                    VenueName = item.Booking.Court?.Venue?.Name ?? "Venue không xác định",
                    OwnerName = item.Booking.Court?.Venue?.OwnerName ?? "Chưa gán chủ sân",
                    CustomerName = item.Booking.CustomerName,
                    CustomerPhone = item.Booking.CustomerPhone,
                    CourtName = item.Booking.Court?.Name ?? "Sân không xác định",
                    TimeLabel = $"{PresentationFormatter.FormatBookingDate(item.Booking.StartAt)} · {PresentationFormatter.FormatTimeRange(item.Booking.StartAt, item.Booking.EndAt)}",
                    TotalLabel = MockData.FormatVND(item.Booking.TotalPrice),
                    Status = item.EffectiveStatus
                })
                .ToList()
        };
    }

    public async Task<OperationResult> ApproveVenueAsync(string id, CancellationToken cancellationToken = default)
    {
        var venue = await _context.Venues.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (venue is null)
        {
            return OperationResult.Fail("Không tìm thấy hồ sơ cụm sân cần cập nhật.");
        }

        if (venue.Status == VenueStatus.Approved)
        {
            return OperationResult.Success($"'{venue.Name}' đã ở trạng thái công khai.");
        }

        venue.Status = VenueStatus.Approved;
        venue.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success($"Đã duyệt '{venue.Name}'. Venue hiện đã xuất hiện trên trang public và sẵn sàng nhận booking mới.");
    }

    public async Task<OperationResult> RejectVenueAsync(string id, CancellationToken cancellationToken = default)
    {
        var venue = await _context.Venues.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (venue is null)
        {
            return OperationResult.Fail("Không tìm thấy hồ sơ cụm sân cần cập nhật.");
        }

        if (venue.Status == VenueStatus.Rejected)
        {
            return OperationResult.Success($"'{venue.Name}' đã ở trạng thái ẩn khỏi public.");
        }

        venue.Status = VenueStatus.Rejected;
        venue.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success($"Đã ẩn '{venue.Name}' khỏi public site. Chủ sân có thể cập nhật lại hồ sơ trước khi gửi duyệt lại.");
    }

    public async Task<OperationResult> LockUserAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (user is null)
        {
            return OperationResult.Fail("Không tìm thấy tài khoản cần khóa.");
        }

        if (user.Role == AppRoles.Admin)
        {
            return OperationResult.Fail("Không thể khóa tài khoản quản trị viên.");
        }

        if (_currentUserService.User?.UserId == id)
        {
            return OperationResult.Fail("Không thể tự khóa tài khoản đang đăng nhập.");
        }

        if (!user.IsActive)
        {
            return OperationResult.Success($"Tài khoản '{user.FullName}' đã ở trạng thái khóa.");
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success($"Đã khóa tài khoản '{user.FullName}'. Người dùng sẽ không thể đăng nhập hoặc thao tác tiếp trên hệ thống.");
    }

    public async Task<OperationResult> UnlockUserAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (user is null)
        {
            return OperationResult.Fail("Không tìm thấy tài khoản cần mở khóa.");
        }

        if (user.Role == AppRoles.Admin)
        {
            return OperationResult.Fail("Không cần mở khóa cho tài khoản quản trị viên.");
        }

        if (user.IsActive)
        {
            return OperationResult.Success($"Tài khoản '{user.FullName}' đang hoạt động bình thường.");
        }

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success($"Đã mở khóa tài khoản '{user.FullName}'. Người dùng có thể đăng nhập lại.");
    }

    private static BookingStatus ResolveBookingStatus(BookingEntity booking)
    {
        return booking.Status != BookingStatus.Cancelled && booking.EndAt < DateTime.Now
            ? BookingStatus.Completed
            : booking.Status;
    }

    private static int GetVenueSortOrder(VenueStatus status) => status switch
    {
        VenueStatus.PendingApproval => 0,
        VenueStatus.Approved => 1,
        _ => 2
    };
}
