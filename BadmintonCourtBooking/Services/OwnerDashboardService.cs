using System.Globalization;
using BadmintonCourtBooking.Data;
using BadmintonCourtBooking.Data.Entities;
using BadmintonCourtBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Services;

public class OwnerDashboardService(ApplicationDbContext context, ICurrentUserService currentUserService) : IOwnerDashboardService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<OwnerDashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var currentUser = _currentUserService.User;
        if (currentUser is null)
        {
            return new OwnerDashboardViewModel();
        }

        var bookings = await QueryOwnerBookings(currentUser.UserId).ToListAsync(cancellationToken);
        var ownerVenues = await _context.Venues
            .AsNoTracking()
            .Include(venue => venue.Courts)
            .Where(venue => venue.OwnerUserId == currentUser.UserId)
            .OrderByDescending(venue => venue.UpdatedAt)
            .ToListAsync(cancellationToken);

        var today = DateTime.Today;
        var totalOwnedCourts = ownerVenues.SelectMany(venue => venue.Courts).Count();
        var activePublicCourts = ownerVenues
            .Where(venue => venue.Status == VenueStatus.Approved)
            .SelectMany(venue => venue.Courts)
            .Count(court => court.IsActive);

        var todayBookings = bookings
            .Where(booking => booking.StartAt.Date == today && booking.Status != BookingStatus.Cancelled)
            .ToList();

        var confirmedTodayCount = todayBookings.Count(booking =>
        {
            var status = ResolveBookingStatus(booking);
            return status is BookingStatus.Confirmed or BookingStatus.Completed;
        });
        var pendingTodayCount = todayBookings.Count(booking => booking.Status == BookingStatus.Pending);
        var totalSlotsToday = activePublicCourts * MockData.TimeSlots.Count;
        var availableTodayCount = Math.Max(totalSlotsToday - confirmedTodayCount - pendingTodayCount, 0);

        return new OwnerDashboardViewModel
        {
            OwnerName = currentUser.FullName,
            OverviewText = BuildOverviewText(ownerVenues),
            TotalBookings = bookings.Count,
            TodayBookings = todayBookings.Count,
            PendingCount = bookings.Count(booking => booking.Status == BookingStatus.Pending),
            Revenue = bookings
                .Where(booking =>
                {
                    var status = ResolveBookingStatus(booking);
                    return status is BookingStatus.Confirmed or BookingStatus.Completed;
                })
                .Sum(booking => booking.TotalPrice),
            ActiveCourts = $"{activePublicCourts}/{totalOwnedCourts}",
            SlotDistribution = new OwnerSlotDistributionViewModel
            {
                Confirmed = confirmedTodayCount,
                Pending = pendingTodayCount,
                Available = availableTodayCount
            },
            RevenueData = BuildRevenueData(bookings, today),
            Bookings = OrderDashboardBookings(bookings)
                .Select(MapOwnerBooking)
                .ToList()
        };
    }

    public async Task<OwnerVenueManagementViewModel> GetVenueManagementAsync(string? selectedVenueId = null, CancellationToken cancellationToken = default)
    {
        var currentUser = _currentUserService.User;
        if (currentUser is null)
        {
            return new OwnerVenueManagementViewModel();
        }

        var venues = await _context.Venues
            .AsNoTracking()
            .Include(venue => venue.Courts)
            .Where(venue => venue.OwnerUserId == currentUser.UserId)
            .OrderByDescending(venue => venue.UpdatedAt)
            .ThenBy(venue => venue.Name)
            .ToListAsync(cancellationToken);

        var mappedVenues = venues.Select(MapManagedVenue).ToList();
        var resolvedSelectedVenueId = mappedVenues.Any(venue => venue.Id == selectedVenueId)
            ? selectedVenueId!
            : mappedVenues.FirstOrDefault()?.Id ?? string.Empty;

        return new OwnerVenueManagementViewModel
        {
            OwnerName = currentUser.FullName,
            SelectedVenueId = resolvedSelectedVenueId,
            Venues = mappedVenues
        };
    }

    public async Task<OperationResult<string>> CreateVenueAsync(OwnerVenueInputModel model, CancellationToken cancellationToken = default)
    {
        var currentUser = _currentUserService.User;
        if (currentUser is null)
        {
            return OperationResult<string>.Fail("Bạn cần đăng nhập bằng tài khoản chủ sân để tạo cụm sân.");
        }

        if (!TryBuildOpenHours(model.OpenTime, model.CloseTime, out var openHours, out var hoursError))
        {
            return OperationResult<string>.Fail(hoursError);
        }

        var name = model.Name.Trim();
        var district = model.District.Trim();
        var address = model.Address.Trim();
        var description = model.Description.Trim();

        var duplicateExists = await _context.Venues.AnyAsync(
            venue => venue.OwnerUserId == currentUser.UserId
                && venue.Name == name
                && venue.Address == address,
            cancellationToken);

        if (duplicateExists)
        {
            return OperationResult<string>.Fail("Bạn đã có một cụm sân trùng tên và địa chỉ này.");
        }

        var venue = new VenueEntity
        {
            Id = $"venue-{Guid.NewGuid():N}",
            Name = name,
            District = district,
            Address = address,
            OpenHours = openHours,
            OwnerUserId = currentUser.UserId,
            OwnerName = currentUser.FullName,
            OwnerPhone = currentUser.PhoneNumber,
            Description = description,
            Highlight = "Chờ quản trị viên duyệt hồ sơ",
            Rating = 0,
            Reviews = 0,
            ResponseFast = true,
            HasSlotsToday = false,
            Status = VenueStatus.PendingApproval,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Venues.Add(venue);
        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult<string>.Success(
            venue.Id,
            $"Đã tạo hồ sơ cụm sân '{venue.Name}'. Venue đang ở trạng thái chờ duyệt.");
    }

    public async Task<OperationResult<string>> UpdateVenueAsync(OwnerVenueInputModel model, CancellationToken cancellationToken = default)
    {
        var currentUser = _currentUserService.User;
        if (currentUser is null)
        {
            return OperationResult<string>.Fail("Bạn cần đăng nhập để cập nhật cụm sân.");
        }

        if (string.IsNullOrWhiteSpace(model.Id))
        {
            return OperationResult<string>.Fail("Không tìm thấy cụm sân cần cập nhật.");
        }

        if (!TryBuildOpenHours(model.OpenTime, model.CloseTime, out var openHours, out var hoursError))
        {
            return OperationResult<string>.Fail(hoursError);
        }

        var venue = await FindOwnedVenueAsync(model.Id, cancellationToken);
        if (venue is null)
        {
            return OperationResult<string>.Fail("Bạn không có quyền cập nhật cụm sân này.");
        }

        var name = model.Name.Trim();
        var district = model.District.Trim();
        var address = model.Address.Trim();
        var description = model.Description.Trim();

        var duplicateExists = await _context.Venues.AnyAsync(
            item => item.OwnerUserId == currentUser.UserId
                && item.Id != venue.Id
                && item.Name == name
                && item.Address == address,
            cancellationToken);

        if (duplicateExists)
        {
            return OperationResult<string>.Fail("Đã có cụm sân khác của bạn dùng cùng tên và địa chỉ này.");
        }

        var needsResubmission = venue.Status == VenueStatus.Rejected;

        venue.Name = name;
        venue.District = district;
        venue.Address = address;
        venue.OpenHours = openHours;
        venue.Description = description;
        venue.OwnerName = currentUser.FullName;
        venue.OwnerPhone = currentUser.PhoneNumber;
        venue.Highlight = needsResubmission ? "Đã cập nhật lại hồ sơ chờ duyệt" : venue.Highlight;
        venue.Status = needsResubmission ? VenueStatus.PendingApproval : venue.Status;
        venue.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult<string>.Success(
            venue.Id,
            needsResubmission
                ? $"Đã cập nhật '{venue.Name}' và gửi lại hồ sơ để quản trị viên duyệt."
                : $"Đã cập nhật thông tin cụm sân '{venue.Name}'.");
    }

    public async Task<OperationResult<string>> CreateCourtAsync(OwnerCourtInputModel model, CancellationToken cancellationToken = default)
    {
        if (_currentUserService.User is null)
        {
            return OperationResult<string>.Fail("Bạn cần đăng nhập để thêm sân con.");
        }

        var venue = await FindOwnedVenueAsync(model.VenueId, cancellationToken);
        if (venue is null)
        {
            return OperationResult<string>.Fail("Bạn không có quyền thêm sân con vào cụm sân này.");
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

    public async Task<OperationResult<string>> UpdateCourtAsync(OwnerCourtInputModel model, CancellationToken cancellationToken = default)
    {
        if (_currentUserService.User is null)
        {
            return OperationResult<string>.Fail("Bạn cần đăng nhập để cập nhật sân con.");
        }

        if (string.IsNullOrWhiteSpace(model.Id))
        {
            return OperationResult<string>.Fail("Không tìm thấy sân con cần cập nhật.");
        }

        var court = await FindOwnedCourtAsync(model.Id, cancellationToken);
        if (court?.Venue is null)
        {
            return OperationResult<string>.Fail("Bạn không có quyền cập nhật sân con này.");
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
        return OperationResult<string>.Success(
            court.VenueId,
            $"{actionText} sân con '{court.Name}'.");
    }

    public async Task<OperationResult> ApproveBookingAsync(string id, CancellationToken cancellationToken = default)
    {
        if (_currentUserService.User is null)
        {
            return OperationResult.Fail("Bạn cần đăng nhập để thực hiện thao tác này.");
        }

        var booking = await FindOwnerBookingAsync(id, cancellationToken);
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
        if (_currentUserService.User is null)
        {
            return OperationResult.Fail("Bạn cần đăng nhập để thực hiện thao tác này.");
        }

        var booking = await FindOwnerBookingAsync(id, cancellationToken);
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
        booking.CancelReason = "Từ chối bởi chủ sân";
        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success($"Đã từ chối lịch đặt sân của {booking.CustomerName}.");
    }

    private IQueryable<OwnerBookingRecord> QueryOwnerBookings(string currentUserId)
    {
        return _context.Bookings
            .AsNoTracking()
            .Where(booking => booking.Court != null
                && booking.Court.Venue != null
                && booking.Court.Venue.OwnerUserId == currentUserId)
            .Select(booking => new OwnerBookingRecord(
                booking.Id,
                booking.Court!.Venue!.Name,
                booking.CustomerName,
                booking.CustomerPhone,
                booking.Court.Name,
                booking.StartAt,
                booking.EndAt,
                booking.TotalPrice,
                booking.Status));
    }

    private async Task<VenueEntity?> FindOwnedVenueAsync(string? venueId, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.User?.UserId;
        if (string.IsNullOrWhiteSpace(currentUserId) || string.IsNullOrWhiteSpace(venueId))
        {
            return null;
        }

        return await _context.Venues.FirstOrDefaultAsync(
            venue => venue.Id == venueId && venue.OwnerUserId == currentUserId,
            cancellationToken);
    }

    private async Task<CourtEntity?> FindOwnedCourtAsync(string courtId, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.User?.UserId;
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return null;
        }

        return await _context.Courts
            .Include(court => court.Venue)
            .Include(court => court.Bookings)
            .FirstOrDefaultAsync(
                court => court.Id == courtId
                    && court.Venue != null
                    && court.Venue.OwnerUserId == currentUserId,
                cancellationToken);
    }

    private async Task<BookingEntity?> FindOwnerBookingAsync(string id, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.User?.UserId;
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return null;
        }

        return await _context.Bookings
            .Include(booking => booking.Court)
                .ThenInclude(court => court!.Venue)
            .FirstOrDefaultAsync(booking => booking.Id == id
                && booking.Court != null
                && booking.Court.Venue != null
                && booking.Court.Venue.OwnerUserId == currentUserId, cancellationToken);
    }

    private static List<RevenueDataPoint> BuildRevenueData(IEnumerable<OwnerBookingRecord> bookings, DateTime today)
    {
        return Enumerable.Range(0, 7)
            .Select(offset =>
            {
                var day = today.AddDays(offset - 6);
                var total = bookings
                    .Where(booking => booking.StartAt.Date == day)
                    .Where(booking =>
                    {
                        var status = ResolveBookingStatus(booking);
                        return status is BookingStatus.Confirmed or BookingStatus.Completed;
                    })
                    .Sum(booking => booking.TotalPrice);

                return new RevenueDataPoint
                {
                    Day = FormatShortDay(day),
                    Value = Math.Round(total / 1_000_000d, 1)
                };
            })
            .ToList();
    }

    private static IEnumerable<OwnerBookingRecord> OrderDashboardBookings(IEnumerable<OwnerBookingRecord> bookings)
    {
        return bookings
            .OrderBy(booking => booking.Status == BookingStatus.Pending ? 0 : 1)
            .ThenBy(booking => booking.Status == BookingStatus.Pending ? booking.StartAt : DateTime.MaxValue)
            .ThenByDescending(booking => booking.StartAt);
    }

    private static OwnerBookingViewModel MapOwnerBooking(OwnerBookingRecord booking)
    {
        var status = ResolveBookingStatus(booking);

        return new OwnerBookingViewModel
        {
            Id = booking.Id,
            VenueName = booking.VenueName,
            Customer = booking.CustomerName,
            Phone = PresentationFormatter.MaskPhone(booking.CustomerPhone),
            Court = booking.CourtName,
            Time = $"{PresentationFormatter.FormatBookingDate(booking.StartAt)} · {PresentationFormatter.FormatTimeRange(booking.StartAt, booking.EndAt)}",
            Total = booking.TotalPrice,
            Status = status
        };
    }

    private static OwnerVenueViewModel MapManagedVenue(VenueEntity venue)
    {
        var (openTime, closeTime) = SplitOpenHours(venue.OpenHours);
        var activeCourts = venue.Courts.Where(court => court.IsActive).ToList();

        return new OwnerVenueViewModel
        {
            Id = venue.Id,
            Name = venue.Name,
            District = venue.District,
            Address = venue.Address,
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
                .Select(court => new OwnerCourtViewModel
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

    private static string BuildOverviewText(IEnumerable<VenueEntity> ownerVenues)
    {
        var venues = ownerVenues.ToList();
        if (venues.Count == 0)
        {
            return "Bạn chưa có cụm sân nào. Tạo venue đầu tiên để bắt đầu quy trình duyệt và nhận booking.";
        }

        var parts = new List<string>();
        var approvedCount = venues.Count(venue => venue.Status == VenueStatus.Approved);
        var pendingCount = venues.Count(venue => venue.Status == VenueStatus.PendingApproval);
        var rejectedCount = venues.Count(venue => venue.Status == VenueStatus.Rejected);

        if (approvedCount > 0)
        {
            parts.Add($"{approvedCount} cụm sân đang công khai");
        }

        if (pendingCount > 0)
        {
            parts.Add($"{pendingCount} hồ sơ chờ duyệt");
        }

        if (rejectedCount > 0)
        {
            parts.Add($"{rejectedCount} hồ sơ cần cập nhật lại");
        }

        return string.Join(" · ", parts);
    }

    private static BookingStatus ResolveBookingStatus(OwnerBookingRecord booking)
    {
        return booking.Status != BookingStatus.Cancelled && booking.EndAt < DateTime.Now
            ? BookingStatus.Completed
            : booking.Status;
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

    private static string FormatShortDay(DateTime date) => date.DayOfWeek switch
    {
        DayOfWeek.Monday => "T2",
        DayOfWeek.Tuesday => "T3",
        DayOfWeek.Wednesday => "T4",
        DayOfWeek.Thursday => "T5",
        DayOfWeek.Friday => "T6",
        DayOfWeek.Saturday => "T7",
        _ => "CN"
    };

    private sealed record OwnerBookingRecord(
        string Id,
        string VenueName,
        string CustomerName,
        string CustomerPhone,
        string CourtName,
        DateTime StartAt,
        DateTime EndAt,
        int TotalPrice,
        BookingStatus Status);
}
