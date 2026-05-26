using System.Globalization;
using BadmintonCourtBooking.Data.Entities;
using BadmintonCourtBooking.Models;
using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Data;

public class ApplicationDbContextSeeder(ApplicationDbContext context, IPasswordHasher<AppUserEntity> passwordHasher)
{
    private readonly ApplicationDbContext _context = context;
    private readonly IPasswordHasher<AppUserEntity> _passwordHasher = passwordHasher;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedUsersAsync(cancellationToken);
        await SeedVenuesAsync(cancellationToken);
        await SeedUserSnapshotsAsync(cancellationToken);
        await SeedBookingsAsync(cancellationToken);
        await SyncVenueOwnersAsync(cancellationToken);
        await SyncBookingPlayersAsync(cancellationToken);
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        var existingUsers = await _context.Users.ToListAsync(cancellationToken);
        var hasChanges = false;

        foreach (var seed in BuildSeedAccounts())
        {
            var user = existingUsers.FirstOrDefault(item => item.Id == seed.Id)
                ?? existingUsers.FirstOrDefault(item => item.NormalizedEmail == AccountValueNormalizer.NormalizeEmail(seed.Email))
                ?? existingUsers.FirstOrDefault(item => item.NormalizedPhoneNumber == AccountValueNormalizer.NormalizePhone(seed.PhoneNumber));

            if (user is not null)
            {
                continue;
            }

            user = new AppUserEntity
            {
                Id = seed.Id,
                FullName = seed.FullName,
                Email = seed.Email,
                NormalizedEmail = AccountValueNormalizer.NormalizeEmail(seed.Email),
                PhoneNumber = seed.PhoneNumber,
                NormalizedPhoneNumber = AccountValueNormalizer.NormalizePhone(seed.PhoneNumber),
                Role = seed.Role,
                PlayArea = seed.PlayArea,
                JoinedAt = seed.JoinedAt,
                UpdatedAt = seed.JoinedAt,
                IsActive = true,
                IsPhoneVerified = true,
                ReceiveBookingConfirm = true,
                ReceivePlayReminder = true,
                ReceivePromo = seed.ReceivePromo
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, seed.Password);

            existingUsers.Add(user);
            _context.Users.Add(user);
            hasChanges = true;
        }

        if (hasChanges)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task SeedVenuesAsync(CancellationToken cancellationToken)
    {
        if (await _context.Venues.AnyAsync(cancellationToken))
        {
            return;
        }

        var venues = MockData.Venues.Select(MapApprovedVenue).ToList();
        venues.AddRange(MockData.PendingVenues.Select(MapPendingVenue));

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

        foreach (var pending in MockData.PendingVenues)
        {
            for (var index = 1; index <= pending.Courts; index++)
            {
                courts.Add(new CourtEntity
                {
                    Id = BuildCourtId(pending.Id, $"c{index}"),
                    VenueId = pending.Id,
                    Name = $"Sân {index}",
                    PricePerHour = 90_000,
                    Note = index == 1 ? "Sân demo" : null,
                    IsActive = true
                });
            }
        }

        await _context.Venues.AddRangeAsync(venues, cancellationToken);
        await _context.Courts.AddRangeAsync(courts, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedUserSnapshotsAsync(CancellationToken cancellationToken)
    {
        if (await _context.UserSnapshots.AnyAsync(cancellationToken))
        {
            return;
        }

        var users = MockData.RecentUsers.Select((user, index) => new UserSnapshotEntity
        {
            Id = $"user-{index + 1}",
            Name = user.Name,
            JoinedAt = DateTime.ParseExact(user.JoinDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
            RoleLabel = user.Role
        });

        await _context.UserSnapshots.AddRangeAsync(users, cancellationToken);
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

        foreach (var booking in MockData.OwnerBookings)
        {
            var courtId = BuildCourtId(DemoDataConstants.DemoOwnerVenueId, ResolveOwnerCourtKey(booking.Court));
            var (startAt, endAt) = ParseRelativeBooking(booking.Time);
            bookings.Add(new BookingEntity
            {
                Id = booking.Id,
                CourtId = courtId,
                PlayerUserId = ResolveBookingUserId(booking.Customer),
                CustomerName = booking.Customer,
                CustomerPhone = booking.Phone,
                StartAt = startAt,
                EndAt = endAt,
                TotalPrice = booking.Total,
                Status = booking.Status,
                CreatedAt = startAt.AddHours(-6),
                CancelledAt = booking.Status == BookingStatus.Cancelled ? startAt.AddHours(-2) : null,
                CancelReason = booking.Status == BookingStatus.Cancelled ? "Từ chối bởi chủ sân" : null
            });
        }

        await _context.Bookings.AddRangeAsync(bookings, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SyncVenueOwnersAsync(CancellationToken cancellationToken)
    {
        var venues = await _context.Venues.ToListAsync(cancellationToken);
        var hasChanges = false;

        foreach (var venue in venues)
        {
            var ownerUserId = ResolveOwnerUserId(venue.Id);
            if (!string.IsNullOrWhiteSpace(ownerUserId) && venue.OwnerUserId != ownerUserId)
            {
                venue.OwnerUserId = ownerUserId;
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
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
        var (ownerName, ownerPhone) = ResolveOwner(venue.Id);

        return new VenueEntity
        {
            Id = venue.Id,
            Name = venue.Name,
            District = venue.District,
            Address = venue.Address,
            OpenHours = venue.OpenHours,
            OwnerUserId = ResolveOwnerUserId(venue.Id),
            OwnerName = ownerName,
            OwnerPhone = ownerPhone,
            Description = venue.Description,
            Highlight = venue.Highlight,
            Rating = venue.Rating,
            Reviews = venue.Reviews,
            ResponseFast = venue.ResponseFast,
            HasSlotsToday = venue.HasSlotsToday,
            Status = VenueStatus.Approved,
            CreatedAt = DateTime.UtcNow.AddDays(-14),
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        };
    }

    private static VenueEntity MapPendingVenue(PendingVenue pendingVenue)
    {
        return new VenueEntity
        {
            Id = pendingVenue.Id,
            Name = pendingVenue.Name,
            District = pendingVenue.District,
            Address = $"Địa chỉ đang thẩm định, {pendingVenue.District}",
            OpenHours = "06:00 – 22:00",
            OwnerUserId = ResolveOwnerUserId(pendingVenue.Id),
            OwnerName = pendingVenue.Owner,
            OwnerPhone = "0900000000",
            Description = $"Hồ sơ đăng ký venue mới của {pendingVenue.Owner}. Đang chờ admin phê duyệt thông tin pháp lý và chất lượng sân.",
            Highlight = "Đang chờ duyệt",
            Rating = 0,
            Reviews = 0,
            ResponseFast = false,
            HasSlotsToday = false,
            Status = VenueStatus.PendingApproval,
            CreatedAt = DateTime.ParseExact(pendingVenue.Submitted, "dd/MM/yyyy", CultureInfo.InvariantCulture),
            UpdatedAt = DateTime.ParseExact(pendingVenue.Submitted, "dd/MM/yyyy", CultureInfo.InvariantCulture)
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

    private static (DateTime startAt, DateTime endAt) ParseRelativeBooking(string timeText)
    {
        var parts = timeText.Split(" · ", StringSplitOptions.TrimEntries);
        var dayToken = parts[0];
        var start = TimeSpan.Parse(parts[1], CultureInfo.InvariantCulture);
        var date = dayToken switch
        {
            "Hôm nay" => DateTime.Today,
            "Mai" => DateTime.Today.AddDays(1),
            _ => DateTime.Today
        };

        return (date.Date.Add(start), date.Date.Add(start).AddHours(1));
    }

    private static (TimeSpan start, TimeSpan end) ParseTimeRange(string timeText)
    {
        var parts = timeText.Split(" – ", StringSplitOptions.TrimEntries);
        var start = TimeSpan.Parse(parts[0], CultureInfo.InvariantCulture);
        var end = TimeSpan.Parse(parts[1], CultureInfo.InvariantCulture);
        return (start, end);
    }

    private static string BuildCourtId(string venueId, string courtId) => $"{venueId}-{courtId}";

    private static string ResolveOwnerCourtKey(string courtName) => courtName switch
    {
        "Sân 1" => "c1",
        "Sân 2" => "c2",
        "Sân 3" => "c3",
        "Sân VIP" => "c4",
        _ => "c1"
    };

    private static (string ownerName, string ownerPhone) ResolveOwner(string venueId) => venueId switch
    {
        "v1" or "v4" => (DemoDataConstants.DemoOwnerName, DemoDataConstants.DemoOwnerPhone),
        "v2" or "v3" => ("Trần Quốc Vũ", "0987123456"),
        "v5" or "v6" => ("Lê Minh Hải", "0911222333"),
        "pv1" => ("Trần Văn Hùng", "0908000001"),
        "pv2" => ("Lê Thị Mai", "0908000002"),
        "pv3" => ("Phạm Quốc Việt", "0908000003"),
        _ => ("Chủ sân CourtBook", "0900000000")
    };

    private static string? ResolveOwnerUserId(string venueId) => venueId switch
    {
        "v1" or "v4" => DemoDataConstants.DemoOwnerUserId,
        "v2" or "v3" => "user-owner-vu",
        "v5" or "v6" => "user-owner-hai",
        "pv1" => "user-owner-hung",
        "pv2" => "user-owner-mai",
        "pv3" => "user-owner-viet",
        _ => null
    };

    private static string? ResolveBookingUserId(string customerName) => customerName switch
    {
        DemoDataConstants.DemoPlayerName => DemoDataConstants.DemoPlayerUserId,
        "Trần Phương Linh" => "user-owner-linh",
        "Lê Quốc Bảo" => "user-player-bao",
        "Phạm Hồng Nhung" => "user-player-nhung",
        "Đặng Tuấn Anh" => "user-owner-tuan-anh",
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
            DemoDataConstants.DemoOwnerUserId,
            DemoDataConstants.DemoOwnerName,
            DemoDataConstants.DemoOwnerEmail,
            DemoDataConstants.DemoOwnerPhone,
            AppRoles.Owner,
            DemoDataConstants.DemoOwnerPassword,
            "Quận 11, TP.HCM",
            new DateTime(2025, 2, 15),
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

        yield return new SeedAccount("user-owner-vu", "Trần Quốc Vũ", "vu.owner@courtbook.local", "0987123456", AppRoles.Owner, "CourtBook@123", "Quận Tân Bình", new DateTime(2025, 2, 20), false);
        yield return new SeedAccount("user-owner-hai", "Lê Minh Hải", "hai.owner@courtbook.local", "0911222333", AppRoles.Owner, "CourtBook@123", "Gò Vấp", new DateTime(2025, 2, 22), false);
        yield return new SeedAccount("user-owner-hung", "Trần Văn Hùng", "hung.owner@courtbook.local", "0908000001", AppRoles.Owner, "CourtBook@123", "Quận 2", new DateTime(2025, 5, 18), false);
        yield return new SeedAccount("user-owner-mai", "Lê Thị Mai", "mai.owner@courtbook.local", "0908000002", AppRoles.Owner, "CourtBook@123", "Bình Tân", new DateTime(2025, 5, 17), false);
        yield return new SeedAccount("user-owner-viet", "Phạm Quốc Việt", "viet.owner@courtbook.local", "0908000003", AppRoles.Owner, "CourtBook@123", "Tân Bình", new DateTime(2025, 5, 16), false);
        yield return new SeedAccount("user-owner-linh", "Trần Phương Linh", "linh.owner@courtbook.local", "0987000104", AppRoles.Owner, "CourtBook@123", "TP. Thủ Đức", new DateTime(2026, 5, 19), false);
        yield return new SeedAccount("user-player-bao", "Lê Quốc Bảo", "bao.player@courtbook.local", "0912000876", AppRoles.Player, "CourtBook@123", "Bình Thạnh", new DateTime(2026, 5, 18), false);
        yield return new SeedAccount("user-player-nhung", "Phạm Hồng Nhung", "nhung.player@courtbook.local", "0934000012", AppRoles.Player, "CourtBook@123", "Quận 10", new DateTime(2026, 5, 17), true);
        yield return new SeedAccount("user-owner-tuan-anh", "Đặng Tuấn Anh", "tuananh.owner@courtbook.local", "0976000455", AppRoles.Owner, "CourtBook@123", "Quận 11", new DateTime(2026, 5, 16), false);
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
}
