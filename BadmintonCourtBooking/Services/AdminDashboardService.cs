using System.Globalization;
using BadmintonCourtBooking.Data;
using BadmintonCourtBooking.Data.Entities;
using BadmintonCourtBooking.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Services;

public class AdminDashboardService(
    ApplicationDbContext context,
    ICurrentUserService currentUserService,
    UserManager<AppUserEntity> userManager) : IAdminDashboardService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly UserManager<AppUserEntity> _userManager = userManager;

    public async Task<AdminDashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var roleMap = await BuildRoleMapAsync(cancellationToken);
        var users = (await _context.Users
            .AsNoTracking()
            .OrderByDescending(user => user.JoinedAt)
            .ToListAsync(cancellationToken))
            .Where(user => !HasRole(roleMap, user.Id, AppRoles.Admin))
            .ToList();

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

        var pendingBookingsByVenue = bookingSummaries
            .Where(item => item.EffectiveStatus == BookingStatus.Pending && !string.IsNullOrWhiteSpace(item.VenueId))
            .GroupBy(item => item.VenueId)
            .ToDictionary(group => group.Key, group => group.Count());

        var today = DateTime.Today;

        return new AdminDashboardViewModel
        {
            AdminName = _currentUserService.User?.FullName ?? "Quản trị viên Đặt Sân Cầu Lông",
            TotalUsers = users.Count,
            TotalPlayers = users.Count(user => HasRole(roleMap, user.Id, AppRoles.Player)),
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
                    ContactName = venue.ContactName,
                    ContactPhone = venue.ContactPhone,
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
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    Role = GetPrimaryRole(roleMap, user.Id),
                    IsActive = user.IsActive,
                    OwnedVenueCount = 0,
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
                    ContactName = venue.ContactName,
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
                    ContactName = item.Booking.Court?.Venue?.ContactName ?? "Chưa gán liên hệ",
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

    public async Task<AdminVenueManagementViewModel> GetVenueManagementAsync(string? selectedVenueId = null, CancellationToken cancellationToken = default)
    {
        var venues = await _context.Venues
            .AsNoTracking()
            .Include(venue => venue.Courts)
            .ToListAsync(cancellationToken);

        var mappedVenues = venues
            .OrderBy(venue => GetVenueSortOrder(venue.Status))
            .ThenByDescending(venue => venue.UpdatedAt)
            .ThenBy(venue => venue.Name)
            .Select(MapManagedVenue)
            .ToList();
        var resolvedSelectedVenueId = mappedVenues.Any(venue => venue.Id == selectedVenueId)
            ? selectedVenueId!
            : mappedVenues.FirstOrDefault()?.Id ?? string.Empty;

        return new AdminVenueManagementViewModel
        {
            AdminName = _currentUserService.User?.FullName ?? "Quản trị viên Đặt Sân Cầu Lông",
            SelectedVenueId = resolvedSelectedVenueId,
            Venues = mappedVenues
        };
    }

    public async Task<OperationResult<string>> CreateVenueAsync(AdminVenueInputModel model, CancellationToken cancellationToken = default)
    {
        if (!TryBuildOpenHours(model.OpenTime, model.CloseTime, out var openHours, out var hoursError))
        {
            return OperationResult<string>.Fail(hoursError);
        }

        var name = model.Name.Trim();
        var district = model.District.Trim();
        var address = model.Address.Trim();
        var contactName = model.ContactName.Trim();
        var contactPhone = model.ContactPhone.Trim();
        var description = model.Description.Trim();

        var duplicateExists = await _context.Venues.AnyAsync(
            venue => venue.Name == name && venue.Address == address,
            cancellationToken);

        if (duplicateExists)
        {
            return OperationResult<string>.Fail("Đã có một cụm sân trùng tên và địa chỉ này.");
        }

        var venue = new VenueEntity
        {
            Id = $"venue-{Guid.NewGuid():N}",
            Name = name,
            District = district,
            Address = address,
            OpenHours = openHours,
            ContactName = contactName,
            ContactPhone = contactPhone,
            Description = description,
            Highlight = "Được quản trị viên xác minh",
            Rating = 0,
            Reviews = 0,
            ResponseFast = true,
            HasSlotsToday = false,
            Status = VenueStatus.Approved,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Venues.Add(venue);
        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult<string>.Success(
            venue.Id,
            $"Đã tạo cụm sân '{venue.Name}'. Venue đang hiển thị công khai.");
    }

    public async Task<OperationResult<string>> UpdateVenueAsync(AdminVenueInputModel model, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(model.Id))
        {
            return OperationResult<string>.Fail("Không tìm thấy cụm sân cần cập nhật.");
        }

        if (!TryBuildOpenHours(model.OpenTime, model.CloseTime, out var openHours, out var hoursError))
        {
            return OperationResult<string>.Fail(hoursError);
        }

        var venue = await _context.Venues.FirstOrDefaultAsync(item => item.Id == model.Id, cancellationToken);
        if (venue is null)
        {
            return OperationResult<string>.Fail("Không tìm thấy cụm sân cần cập nhật.");
        }

        var name = model.Name.Trim();
        var district = model.District.Trim();
        var address = model.Address.Trim();
        var contactName = model.ContactName.Trim();
        var contactPhone = model.ContactPhone.Trim();
        var description = model.Description.Trim();

        var duplicateExists = await _context.Venues.AnyAsync(
            item => item.Id != venue.Id && item.Name == name && item.Address == address,
            cancellationToken);

        if (duplicateExists)
        {
            return OperationResult<string>.Fail("Đã có cụm sân khác dùng cùng tên và địa chỉ này.");
        }

        venue.Name = name;
        venue.District = district;
        venue.Address = address;
        venue.OpenHours = openHours;
        venue.ContactName = contactName;
        venue.ContactPhone = contactPhone;
        venue.Description = description;
        venue.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult<string>.Success(venue.Id, $"Đã cập nhật thông tin cụm sân '{venue.Name}'.");
    }

    public async Task<OperationResult<string>> CreateCourtAsync(AdminCourtInputModel model, CancellationToken cancellationToken = default)
    {
        var venue = await _context.Venues.FirstOrDefaultAsync(item => item.Id == model.VenueId, cancellationToken);
        if (venue is null)
        {
            return OperationResult<string>.Fail("Không tìm thấy cụm sân để thêm sân con.");
        }

        var name = model.Name.Trim();
        var duplicateExists = await _context.Courts.AnyAsync(
            court => court.VenueId == venue.Id && court.Name == name,
            cancellationToken);

        if (duplicateExists)
        {
            return OperationResult<string>.Fail("Tên sân con đã tồn tại trong cụm sân này.");
        }

        var court = new CourtEntity
        {
            Id = $"court-{Guid.NewGuid():N}",
            VenueId = venue.Id,
            Name = name,
            PricePerHour = model.PricePerHour,
            Note = NormalizeNote(model.Note),
            IsActive = true
        };

        _context.Courts.Add(court);
        venue.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult<string>.Success(
            venue.Id,
            $"Đã thêm sân con '{court.Name}' vào cụm sân '{venue.Name}'.");
    }

    public async Task<OperationResult<string>> UpdateCourtAsync(AdminCourtInputModel model, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(model.Id))
        {
            return OperationResult<string>.Fail("Không tìm thấy sân con cần cập nhật.");
        }

        var court = await _context.Courts
            .Include(item => item.Venue)
            .Include(item => item.Bookings)
            .FirstOrDefaultAsync(item => item.Id == model.Id, cancellationToken);

        if (court?.Venue is null)
        {
            return OperationResult<string>.Fail("Không tìm thấy sân con cần cập nhật.");
        }

        var name = model.Name.Trim();
        var duplicateExists = await _context.Courts.AnyAsync(
            item => item.VenueId == court.VenueId
                && item.Id != court.Id
                && item.Name == name,
            cancellationToken);

        if (duplicateExists)
        {
            return OperationResult<string>.Fail("Tên sân con đã tồn tại trong cụm sân này.");
        }

        if (!model.IsActive && court.IsActive && court.Bookings.Any(booking =>
                booking.Status != BookingStatus.Cancelled &&
                booking.EndAt > DateTime.Now))
        {
            return OperationResult<string>.Fail("Không thể tạm ngưng sân đang có booking chưa hoàn tất.");
        }

        court.Name = name;
        court.PricePerHour = model.PricePerHour;
        court.Note = NormalizeNote(model.Note);
        court.IsActive = model.IsActive;
        court.Venue.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var actionText = court.IsActive ? "Đã lưu cập nhật" : "Đã tạm ngưng";
        return OperationResult<string>.Success(court.VenueId, $"{actionText} sân con '{court.Name}'.");
    }

    public async Task<OperationResult> ApproveBookingAsync(string id, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (booking is null)
        {
            return OperationResult.Fail("Không tìm thấy thông tin lịch đặt sân.");
        }

        if (booking.Status != BookingStatus.Pending)
        {
            return OperationResult.Fail("Chỉ có thể duyệt booking đang chờ xác nhận.");
        }

        if (booking.EndAt <= DateTime.Now)
        {
            return OperationResult.Fail("Booking này đã qua thời gian xử lý.");
        }

        booking.Status = BookingStatus.Confirmed;
        booking.CancelledAt = null;
        booking.CancelReason = null;
        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success($"Đã duyệt lịch đặt sân thành công cho {booking.CustomerName}.");
    }

    public async Task<OperationResult> RejectBookingAsync(string id, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (booking is null)
        {
            return OperationResult.Fail("Không tìm thấy thông tin lịch đặt sân.");
        }

        if (booking.Status != BookingStatus.Pending)
        {
            return OperationResult.Fail("Chỉ có thể từ chối booking đang chờ xác nhận.");
        }

        if (booking.EndAt <= DateTime.Now)
        {
            return OperationResult.Fail("Booking này đã qua thời gian xử lý.");
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAt = DateTime.UtcNow;
        booking.CancelReason = "Từ chối bởi quản trị viên";
        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success($"Đã từ chối lịch đặt sân của {booking.CustomerName}.");
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

        return OperationResult.Success($"Đã ẩn '{venue.Name}'. Venue đã được ẩn khỏi trang public.");
    }

    public async Task<OperationResult> LockUserAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return OperationResult.Fail("Không tìm thấy tài khoản cần khóa.");
        }

        if (await _userManager.IsInRoleAsync(user, AppRoles.Admin))
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
        await _userManager.UpdateAsync(user);

        return OperationResult.Success($"Đã khóa tài khoản '{user.FullName}'. Người dùng sẽ không thể đăng nhập hoặc thao tác tiếp trên hệ thống.");
    }

    public async Task<OperationResult> UnlockUserAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return OperationResult.Fail("Không tìm thấy tài khoản cần mở khóa.");
        }

        if (await _userManager.IsInRoleAsync(user, AppRoles.Admin))
        {
            return OperationResult.Fail("Không cần mở khóa cho tài khoản quản trị viên.");
        }

        if (user.IsActive)
        {
            return OperationResult.Success($"Tài khoản '{user.FullName}' đang hoạt động bình thường.");
        }

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return OperationResult.Success($"Đã mở khóa tài khoản '{user.FullName}'. Người dùng có thể đăng nhập lại.");
    }

    private async Task<Dictionary<string, List<string>>> BuildRoleMapAsync(CancellationToken cancellationToken)
    {
        var rows = await _context.UserRoles
            .AsNoTracking()
            .Join(
                _context.Roles.AsNoTracking(),
                userRole => userRole.RoleId,
                role => role.Id,
                (userRole, role) => new { userRole.UserId, role.Name })
            .Where(item => item.Name != null)
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(item => item.UserId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(item => item.Name!).ToList());
    }

    private static bool HasRole(IReadOnlyDictionary<string, List<string>> roleMap, string userId, string role)
    {
        return roleMap.TryGetValue(userId, out var roles) && roles.Contains(role);
    }

    private static string GetPrimaryRole(IReadOnlyDictionary<string, List<string>> roleMap, string userId)
    {
        return roleMap.TryGetValue(userId, out var roles)
            ? roles.FirstOrDefault(role => role == AppRoles.Player) ?? roles.FirstOrDefault() ?? AppRoles.Player
            : AppRoles.Player;
    }

    private static AdminManagedVenueViewModel MapManagedVenue(VenueEntity venue)
    {
        var (openTime, closeTime) = SplitOpenHours(venue.OpenHours);
        var activeCourts = venue.Courts.Where(court => court.IsActive).ToList();

        return new AdminManagedVenueViewModel
        {
            Id = venue.Id,
            Name = venue.Name,
            District = venue.District,
            Address = venue.Address,
            ContactName = venue.ContactName,
            ContactPhone = venue.ContactPhone,
            OpenTime = openTime,
            CloseTime = closeTime,
            Description = venue.Description,
            Status = venue.Status,
            PriceFrom = activeCourts.Min(court => (int?)court.PricePerHour)
                ?? venue.Courts.Min(court => (int?)court.PricePerHour)
                ?? 0,
            ActiveCourtCount = activeCourts.Count,
            TotalCourtCount = venue.Courts.Count,
            Courts = venue.Courts
                .OrderByDescending(court => court.IsActive)
                .ThenBy(court => court.Name)
                .Select(court => new AdminCourtViewModel
                {
                    Id = court.Id,
                    VenueId = venue.Id,
                    Name = court.Name,
                    PricePerHour = court.PricePerHour,
                    Note = court.Note,
                    IsActive = court.IsActive
                })
                .ToList()
        };
    }

    private static (string OpenTime, string CloseTime) SplitOpenHours(string openHours)
    {
        var parts = openHours.Split(" – ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 2
            ? (parts[0], parts[1])
            : ("06:00", "22:00");
    }

    private static bool TryBuildOpenHours(string openTime, string closeTime, out string openHours, out string errorMessage)
    {
        openHours = string.Empty;
        errorMessage = string.Empty;

        var openIsValid = TimeSpan.TryParseExact(openTime, @"hh\:mm", CultureInfo.InvariantCulture, out var openValue);
        var closeIsValid = TimeSpan.TryParseExact(closeTime, @"hh\:mm", CultureInfo.InvariantCulture, out var closeValue);

        if (!openIsValid || !closeIsValid)
        {
            errorMessage = "Giờ mở cửa và đóng cửa phải theo định dạng HH:mm.";
            return false;
        }

        if (openValue >= closeValue)
        {
            errorMessage = "Giờ đóng cửa phải lớn hơn giờ mở cửa.";
            return false;
        }

        openHours = $"{openValue:hh\\:mm} – {closeValue:hh\\:mm}";
        return true;
    }

    private static string? NormalizeNote(string? note)
    {
        return string.IsNullOrWhiteSpace(note) ? null : note.Trim();
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
