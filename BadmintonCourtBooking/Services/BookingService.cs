using System.Globalization;
using BadmintonCourtBooking.Data;
using BadmintonCourtBooking.Data.Entities;
using BadmintonCourtBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Services;

public class BookingService(ApplicationDbContext context, ICurrentUserService currentUserService) : IBookingService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<List<Booking>> GetPlayerBookingsAsync(CancellationToken cancellationToken = default)
    {
        var currentUser = _currentUserService.User;
        if (currentUser is null)
        {
            return new List<Booking>();
        }

        var bookings = await _context.Bookings
            .AsNoTracking()
            .Include(booking => booking.Court)
                .ThenInclude(court => court!.Venue)
            .Where(booking => booking.PlayerUserId == currentUser.UserId)
            .OrderByDescending(booking => booking.StartAt)
            .ToListAsync(cancellationToken);

        return bookings.Select(MapBooking).ToList();
    }

    public async Task<OperationResult> CreateBookingAsync(CreateBookingInputModel model, CancellationToken cancellationToken = default)
    {
        var currentUser = _currentUserService.User;
        if (currentUser is null)
        {
            return OperationResult.Fail("Bạn cần đăng nhập bằng tài khoản người chơi để đặt sân.");
        }

        if (!TryParseBookingWindow(model, out var startAt, out var endAt))
        {
            return OperationResult.Fail("Khung giờ bạn chọn không hợp lệ. Vui lòng tải lại trang và thử lại.");
        }

        if (startAt.Date < DateTime.Today || startAt.Date > DateTime.Today.AddDays(6))
        {
            return OperationResult.Fail("Bạn chỉ có thể đặt sân trong khung 7 ngày hiển thị trên hệ thống.");
        }

        if (startAt <= DateTime.Now)
        {
            return OperationResult.Fail("Khung giờ này đã qua. Vui lòng chọn slot khác.");
        }

        var court = await _context.Courts
            .Include(item => item.Venue)
            .FirstOrDefaultAsync(item =>
                item.Id == model.CourtId &&
                item.IsActive &&
                item.VenueId == model.VenueId &&
                item.Venue != null &&
                item.Venue.Status == VenueStatus.Approved,
                cancellationToken);

        if (court?.Venue is null)
        {
            return OperationResult.Fail("Không tìm thấy sân phù hợp để tạo booking.");
        }

        var hasConflict = await _context.Bookings.AnyAsync(item =>
            item.CourtId == court.Id &&
            item.Status != BookingStatus.Cancelled &&
            item.StartAt < endAt &&
            startAt < item.EndAt,
            cancellationToken);

        if (hasConflict)
        {
            return OperationResult.Fail("Khung giờ này vừa được giữ bởi người chơi khác. Vui lòng chọn slot khác.");
        }

        var booking = new BookingEntity
        {
            Id = $"bk-{Guid.NewGuid():N}",
            CourtId = court.Id,
            PlayerUserId = currentUser.UserId,
            CustomerName = currentUser.FullName,
            CustomerPhone = currentUser.PhoneNumber,
            StartAt = startAt,
            EndAt = endAt,
            TotalPrice = court.PricePerHour,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return OperationResult.Fail("Khung giờ này vừa được giữ bởi người chơi khác. Vui lòng chọn slot khác.");
        }

        return OperationResult.Success($"Đã gửi yêu cầu đặt {court.Name} tại {court.Venue.Name}. Chủ sân sẽ xác nhận trong ít phút.");
    }

    public async Task<OperationResult> CancelBookingAsync(string id, CancellationToken cancellationToken = default)
    {
        var currentUser = _currentUserService.User;
        if (currentUser is null)
        {
            return OperationResult.Fail("Bạn cần đăng nhập để hủy lịch đặt sân.");
        }

        var booking = await _context.Bookings
            .Include(item => item.Court)
                .ThenInclude(court => court!.Venue)
            .FirstOrDefaultAsync(item => item.Id == id
                && item.PlayerUserId == currentUser.UserId, cancellationToken);

        if (booking is null)
        {
            return OperationResult.Fail("Không tìm thấy thông tin lịch đặt sân.");
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return OperationResult.Fail("Lịch đặt sân này đã được hủy trước đó.");
        }

        if (booking.StartAt <= DateTime.Now.AddHours(1))
        {
            return OperationResult.Fail("Chỉ có thể hủy lịch trước giờ chơi ít nhất 1 giờ.");
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAt = DateTime.UtcNow;
        booking.CancelReason = "Hủy bởi người chơi";
        await _context.SaveChangesAsync(cancellationToken);

        var venueName = booking.Court?.Venue?.Name ?? "sân đã chọn";
        var courtName = booking.Court?.Name ?? "khung sân";
        return OperationResult.Success($"Đã hủy lịch đặt tại {venueName} ({courtName}) thành công.");
    }

    private static bool TryParseBookingWindow(CreateBookingInputModel model, out DateTime startAt, out DateTime endAt)
    {
        startAt = default;
        endAt = default;

        if (!DateTime.TryParseExact(model.BookingDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var bookingDate))
        {
            return false;
        }

        if (!TimeSpan.TryParseExact(model.StartTime, @"hh\:mm", CultureInfo.InvariantCulture, out var startTime))
        {
            return false;
        }

        if (!MockData.TimeSlots.Contains(model.StartTime))
        {
            return false;
        }

        startAt = bookingDate.Date.Add(startTime);
        endAt = startAt.AddHours(1);
        return true;
    }

    private static Booking MapBooking(BookingEntity booking)
    {
        var status = booking.Status != BookingStatus.Cancelled && booking.EndAt < DateTime.Now
            ? BookingStatus.Completed
            : booking.Status;

        return new Booking
        {
            Id = booking.Id,
            VenueId = booking.Court?.Venue?.Id ?? string.Empty,
            Venue = booking.Court?.Venue?.Name ?? string.Empty,
            Location = FormatVenueLocation(booking),
            Court = booking.Court?.Name ?? string.Empty,
            Date = PresentationFormatter.FormatBookingDate(booking.StartAt),
            Time = PresentationFormatter.FormatTimeRange(booking.StartAt, booking.EndAt),
            Total = booking.TotalPrice,
            Status = status,
            CanCancel = status is not BookingStatus.Cancelled
                && status is not BookingStatus.Completed
                && booking.StartAt > DateTime.Now.AddHours(1),
            When = status switch
            {
                BookingStatus.Cancelled => "cancelled",
                BookingStatus.Completed => "completed",
                _ => "upcoming"
            }
        };
    }

    private static string FormatVenueLocation(BookingEntity booking)
    {
        var addressParts = new[]
        {
            booking.Court?.Venue?.Address,
            booking.Court?.Venue?.District
        };

        return string.Join(", ", addressParts.Where(part => !string.IsNullOrWhiteSpace(part)));
    }
}
