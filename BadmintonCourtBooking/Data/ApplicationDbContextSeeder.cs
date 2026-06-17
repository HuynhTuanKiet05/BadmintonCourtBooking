using System.Globalization;
using BadmintonCourtBooking.Data.Entities;
using BadmintonCourtBooking.Models;
using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Data;

public class ApplicationDbContextSeeder(
    ApplicationDbContext context,
    UserManager<AppUserEntity> userManager,
    RoleManager<IdentityRole> roleManager)
{
    private readonly ApplicationDbContext _context = context;
    private readonly UserManager<AppUserEntity> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync();
        await SeedUsersAsync(cancellationToken);
        await SeedVenuesAsync(cancellationToken);
        await SeedBookingsAsync(cancellationToken);
        await SyncBookingPlayersAsync(cancellationToken);
        await SeedNotificationsAsync(cancellationToken);
    }

    private async Task SeedRolesAsync()
    {
        foreach (var roleName in new[] { AppRoles.Admin, AppRoles.Player })
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        var existingUsers = await _userManager.Users.ToListAsync(cancellationToken);

        foreach (var seed in BuildSeedAccounts())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var normalizedEmail = AccountValueNormalizer.NormalizeEmail(seed.Email);
            var normalizedPhone = AccountValueNormalizer.NormalizePhone(seed.PhoneNumber);

            var user = existingUsers.FirstOrDefault(item => item.Id == seed.Id)
                ?? existingUsers.FirstOrDefault(item => item.NormalizedEmail == normalizedEmail)
                ?? existingUsers.FirstOrDefault(item =>
                    AccountValueNormalizer.NormalizePhone(item.PhoneNumber) == normalizedPhone);

            if (user is null)
            {
                user = new AppUserEntity
                {
                    Id = seed.Id,
                    UserName = seed.Email,
                    FullName = seed.FullName,
                    Email = seed.Email,
                    EmailConfirmed = true,
                    PhoneNumber = seed.PhoneNumber,
                    PhoneNumberConfirmed = true,
                    PlayArea = seed.PlayArea,
                    JoinedAt = seed.JoinedAt,
                    UpdatedAt = seed.JoinedAt,
                    IsActive = true,
                    ReceiveBookingConfirm = true,
                    ReceivePlayReminder = true,
                    ReceivePromo = seed.ReceivePromo
                };

                var createResult = await _userManager.CreateAsync(user, seed.Password);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException($"Cannot seed user '{seed.Email}': {BuildIdentityErrorMessage(createResult)}");
                }

                existingUsers.Add(user);
            }
            else
            {
                user.UserName = seed.Email;
                user.FullName = seed.FullName;
                user.Email = seed.Email;
                user.EmailConfirmed = true;
                user.PhoneNumber = seed.PhoneNumber;
                user.PhoneNumberConfirmed = true;
                user.PlayArea = seed.PlayArea;
                user.ReceiveBookingConfirm = true;
                user.ReceivePlayReminder = true;
                user.ReceivePromo = seed.ReceivePromo;
                user.UpdatedAt = DateTime.UtcNow;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    throw new InvalidOperationException($"Cannot update seed user '{seed.Email}': {BuildIdentityErrorMessage(updateResult)}");
                }
            }

            await EnsureUserRoleAsync(user, seed.Role);
        }
    }

    private async Task EnsureUserRoleAsync(AppUserEntity user, string role)
    {
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var oldRole in roles.Where(item => item != role))
        {
            await _userManager.RemoveFromRoleAsync(user, oldRole);
        }

        if (!await _userManager.IsInRoleAsync(user, role))
        {
            var result = await _userManager.AddToRoleAsync(user, role);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Cannot assign role '{role}' to '{user.Email}': {BuildIdentityErrorMessage(result)}");
            }
        }
    }

    private async Task SeedVenuesAsync(CancellationToken cancellationToken)
    {
        var existingVenues = await _context.Venues.ToListAsync(cancellationToken);
        if (existingVenues.Count > 0)
        {
            var hasChanges = false;
            foreach (var venue in existingVenues)
            {
                var seedVenue = MockData.Venues.FirstOrDefault(v => v.Id == venue.Id);
                if (seedVenue != null)
                {
                    var mapped = MapApprovedVenue(seedVenue);
                    if (venue.Latitude != mapped.Latitude || venue.Longitude != mapped.Longitude || venue.ImagePath != mapped.ImagePath)
                    {
                        venue.Latitude = mapped.Latitude;
                        venue.Longitude = mapped.Longitude;
                        venue.ImagePath = mapped.ImagePath;
                        hasChanges = true;
                    }
                }
            }
            if (hasChanges)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            return;
        }

        var venues = MockData.Venues.Select(MapApprovedVenue).ToList();

        var courts = new List<CourtEntity>();
        foreach (var venue in MockData.Venues)
        {
            courts.AddRange(venue.Courts.Select(court => new CourtEntity
            {
                Id = BuildCourtId(venue.Id, court.Id),
                VenueId = venue.Id,
                Name = court.Name,
                PricePerHour = court.PricePerHour,
                Note = court.Note,
                IsActive = true
            }));
        }

        await _context.Venues.AddRangeAsync(venues, cancellationToken);
        await _context.Courts.AddRangeAsync(courts, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedBookingsAsync(CancellationToken cancellationToken)
    {
        if (await _context.Bookings.AnyAsync(cancellationToken))
        {
            return;
        }

        var bookings = new List<BookingEntity>();

        foreach (var item in MockData.MyBookings.Select((booking, index) => new { booking, index }))
        {
            var courtId = await FindCourtIdAsync(item.booking.Venue, item.booking.Court, cancellationToken);
            if (courtId is null)
            {
                continue;
            }

            var (startAt, endAt) = BuildPlayerBookingWindow(item.booking, item.index);
            bookings.Add(new BookingEntity
            {
                Id = item.booking.Id,
                CourtId = courtId,
                PlayerUserId = DemoDataConstants.DemoPlayerUserId,
                CustomerName = DemoDataConstants.DemoPlayerName,
                CustomerPhone = DemoDataConstants.DemoPlayerPhone,
                StartAt = startAt,
                EndAt = endAt,
                TotalPrice = item.booking.Total,
                Status = item.booking.Status,
                CreatedAt = startAt.AddDays(-2)
            });
        }

        await _context.Bookings.AddRangeAsync(bookings, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SyncBookingPlayersAsync(CancellationToken cancellationToken)
    {
        var bookings = await _context.Bookings.ToListAsync(cancellationToken);
        var hasChanges = false;

        foreach (var booking in bookings)
        {
            var playerUserId = booking.Id.StartsWith("b", StringComparison.OrdinalIgnoreCase)
                ? DemoDataConstants.DemoPlayerUserId
                : ResolveBookingUserId(booking.CustomerName);

            if (!string.IsNullOrWhiteSpace(playerUserId) && booking.PlayerUserId != playerUserId)
            {
                booking.PlayerUserId = playerUserId;
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private static VenueEntity MapApprovedVenue(Venue venue)
    {
        var (contactName, contactPhone) = ResolveContact(venue.Id);
        var imagePath = venue.Id switch
        {
            "v1" or "v2" or "v4" or "v6" => $"/uploads/venues/{venue.Id}.png",
            _ => null
        };
        var (latitude, longitude) = venue.Id switch
        {
            "v1" => (10.7679, 106.6543), // Phu Tho Stadium (District 11)
            "v2" => (10.7788, 106.6432), // Tan Binh Badminton Hub
            "v3" => (10.8094, 106.6975), // Binh Thanh
            "v4" => (10.7719, 106.6664), // Quan 10 Sport Center
            "v5" => (10.8242, 106.6804), // Go Vap
            "v6" => (10.8524, 106.7716), // Thu Duc Smash Arena
            _ => (null as double?, null as double?)
        };

        return new VenueEntity
        {
            Id = venue.Id,
            Name = venue.Name,
            District = venue.District,
            Address = venue.Address,
            OpenHours = venue.OpenHours,
            ContactName = contactName,
            ContactPhone = contactPhone,
            Description = venue.Description,
            Highlight = venue.Highlight,
            Rating = venue.Rating,
            Reviews = venue.Reviews,
            ResponseFast = venue.ResponseFast,
            HasSlotsToday = venue.HasSlotsToday,
            Status = VenueStatus.Approved,
            ImagePath = imagePath,
            Latitude = latitude,
            Longitude = longitude,
            CreatedAt = DateTime.UtcNow.AddDays(-14),
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        };
    }

    private async Task<string?> FindCourtIdAsync(string venueName, string courtName, CancellationToken cancellationToken)
    {
        return await _context.Courts
            .Where(court => court.Name == courtName && court.Venue != null && court.Venue.Name == venueName)
            .Select(court => court.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static (DateTime startAt, DateTime endAt) BuildPlayerBookingWindow(Booking booking, int index)
    {
        var (start, end) = ParseTimeRange(booking.Time);
        var baseDate = booking.When switch
        {
            "upcoming" => DateTime.Today.AddDays(index + 1),
            "completed" => DateTime.Today.AddDays(-(index + 3)),
            "cancelled" => DateTime.Today.AddDays(-(index + 7)),
            _ => DateTime.Today
        };

        return (baseDate.Date.Add(start), baseDate.Date.Add(end));
    }

    private static (TimeSpan start, TimeSpan end) ParseTimeRange(string timeText)
    {
        var parts = timeText.Split(" – ", StringSplitOptions.TrimEntries);
        var start = TimeSpan.Parse(parts[0], CultureInfo.InvariantCulture);
        var end = TimeSpan.Parse(parts[1], CultureInfo.InvariantCulture);
        return (start, end);
    }

    private static string BuildCourtId(string venueId, string courtId) => $"{venueId}-{courtId}";

    private static (string contactName, string contactPhone) ResolveContact(string venueId) => venueId switch
    {
        "v1" or "v4" => ("Anh Hoàng Nam", "0905123456"),
        "v2" or "v3" => ("Trần Quốc Vũ", "0987123456"),
        "v5" or "v6" => ("Lê Minh Hải", "0911222333"),
        _ => ("CourtBook", "0900000000")
    };

    private static string? ResolveBookingUserId(string customerName) => customerName switch
    {
        DemoDataConstants.DemoPlayerName => DemoDataConstants.DemoPlayerUserId,
        "Lê Quốc Bảo" => "user-player-bao",
        "Phạm Hồng Nhung" => "user-player-nhung",
        _ => null
    };

    private static IEnumerable<SeedAccount> BuildSeedAccounts()
    {
        yield return new SeedAccount(
            DemoDataConstants.DemoPlayerUserId,
            DemoDataConstants.DemoPlayerName,
            DemoDataConstants.DemoPlayerEmail,
            DemoDataConstants.DemoPlayerPhone,
            AppRoles.Player,
            DemoDataConstants.DemoPlayerPassword,
            "Quận 11, TP.HCM",
            new DateTime(2025, 3, 1),
            false);

        yield return new SeedAccount(
            DemoDataConstants.DemoAdminUserId,
            DemoDataConstants.DemoAdminName,
            DemoDataConstants.DemoAdminEmail,
            DemoDataConstants.DemoAdminPhone,
            AppRoles.Admin,
            DemoDataConstants.DemoAdminPassword,
            "TP.HCM",
            new DateTime(2025, 1, 10),
            false);

        yield return new SeedAccount("user-player-vu", "Trần Quốc Vũ", "vu.player@courtbook.local", "0987123456", AppRoles.Player, "CourtBook@123", "Quận Tân Bình", new DateTime(2025, 2, 20), false);
        yield return new SeedAccount("user-player-hai", "Lê Minh Hải", "hai.player@courtbook.local", "0911222333", AppRoles.Player, "CourtBook@123", "Gò Vấp", new DateTime(2025, 2, 22), false);
        yield return new SeedAccount("user-player-hung", "Trần Văn Hùng", "hung.player@courtbook.local", "0908000001", AppRoles.Player, "CourtBook@123", "Quận 2", new DateTime(2025, 5, 18), false);
        yield return new SeedAccount("user-player-mai", "Lê Thị Mai", "mai.player@courtbook.local", "0908000002", AppRoles.Player, "CourtBook@123", "Bình Tân", new DateTime(2025, 5, 17), false);
        yield return new SeedAccount("user-player-viet", "Phạm Quốc Việt", "viet.player@courtbook.local", "0908000003", AppRoles.Player, "CourtBook@123", "Tân Bình", new DateTime(2025, 5, 16), false);
        yield return new SeedAccount("user-player-linh", "Trần Phương Linh", "linh.player@courtbook.local", "0987000104", AppRoles.Player, "CourtBook@123", "TP. Thủ Đức", new DateTime(2026, 5, 19), false);
        yield return new SeedAccount("user-player-bao", "Lê Quốc Bảo", "bao.player@courtbook.local", "0912000876", AppRoles.Player, "CourtBook@123", "Bình Thạnh", new DateTime(2026, 5, 18), false);
        yield return new SeedAccount("user-player-nhung", "Phạm Hồng Nhung", "nhung.player@courtbook.local", "0934000012", AppRoles.Player, "CourtBook@123", "Quận 10", new DateTime(2026, 5, 17), true);
        yield return new SeedAccount("user-player-tuan-anh", "Đặng Tuấn Anh", "tuananh.player@courtbook.local", "0976000455", AppRoles.Player, "CourtBook@123", "Quận 11", new DateTime(2026, 5, 16), false);
    }

    private static string BuildIdentityErrorMessage(IdentityResult result)
    {
        return string.Join("; ", result.Errors.Select(error => error.Description));
    }

    private sealed record SeedAccount(
        string Id,
        string FullName,
        string Email,
        string PhoneNumber,
        string Role,
        string Password,
        string PlayArea,
        DateTime joinedAt,
        bool receivePromo)
    {
        public DateTime JoinedAt { get; init; } = joinedAt;
        public bool ReceivePromo { get; init; } = receivePromo;
    }

    private async Task SeedNotificationsAsync(CancellationToken cancellationToken)
    {
        if (await _context.Notifications.AnyAsync(cancellationToken))
        {
            return;
        }

        var notifications = new List<NotificationEntity>
        {
            new()
            {
                Id = "notif-1",
                UserId = DemoDataConstants.DemoPlayerUserId,
                Title = "Yêu cầu đặt sân được duyệt!",
                Content = "Sân Phú Thọ - Sân VIP lúc 19:00 đã được xác nhận.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                IsRead = false
            },
            new()
            {
                Id = "notif-2",
                UserId = DemoDataConstants.DemoPlayerUserId,
                Title = "Lịch chơi sắp diễn ra",
                Content = "Lịch chơi tại Tân Bình Badminton Hub bắt đầu sau 1 tiếng.",
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                IsRead = false
            },
            new()
            {
                Id = "notif-3",
                UserId = DemoDataConstants.DemoAdminUserId,
                Title = "Yêu cầu đặt sân mới",
                Content = "Người chơi Nguyễn Minh Khoa đã gửi yêu cầu đặt Sân VIP tại Sân Cầu Lông Phú Thọ.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                IsRead = false
            }
        };

        await _context.Notifications.AddRangeAsync(notifications, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
