using System.Globalization;
using BadmintonCourtBooking.Data;
using BadmintonCourtBooking.Data.Entities;
using BadmintonCourtBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Services;

public class VenueCatalogService(ApplicationDbContext context) : IVenueCatalogService
{
    private static readonly IReadOnlyList<TimeSpan> SlotStarts = MockData.TimeSlots
        .Select(slot => TimeSpan.Parse(slot, CultureInfo.InvariantCulture))
        .ToList();

    private readonly ApplicationDbContext _context = context;

    public async Task<List<Venue>> GetFeaturedVenuesAsync(int count = 3, CancellationToken cancellationToken = default)
    {
        var venues = await GetApprovedVenuesAsync(cancellationToken);
        return venues.Take(count).ToList();
    }

    public Task<List<Venue>> GetApprovedVenuesAsync(CancellationToken cancellationToken = default) =>
        SearchApprovedVenuesAsync(new VenueSearchCriteria(), cancellationToken);

    public async Task<List<Venue>> SearchApprovedVenuesAsync(VenueSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        criteria.Normalize();

        var query = QueryApprovedVenues();

        if (!string.IsNullOrWhiteSpace(criteria.Query))
        {
            var term = criteria.Query;
            query = query.Where(venue =>
                venue.Name.Contains(term) ||
                venue.District.Contains(term) ||
                venue.Address.Contains(term));
        }

        if (!string.Equals(criteria.District, VenueSearchCriteria.AllDistricts, StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(venue => venue.District == criteria.District);
        }

        query = query.Where(venue => venue.Courts.Any(court => court.IsActive && court.PricePerHour <= criteria.MaxPrice));

        var venues = (await query.ToListAsync(cancellationToken))
            .Select(MapVenue)
            .ToList();

        if (criteria.Hours != "any")
        {
            venues = venues
                .Where(venue => MatchesHoursFilter(venue.OpenHours, criteria.Hours))
                .ToList();
        }

        if (criteria.OnlyAvailable)
        {
            venues = venues
                .Where(venue => venue.HasSlotsToday)
                .ToList();
        }

        return SortVenues(venues, criteria);
    }

    public async Task<List<string>> GetApprovedDistrictsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Venues
            .AsNoTracking()
            .Where(venue => venue.Status == VenueStatus.Approved)
            .Select(venue => venue.District)
            .Distinct()
            .OrderBy(district => district)
            .ToListAsync(cancellationToken);
    }

    public async Task<VenueDetailViewModel?> GetVenueDetailAsync(string id, CancellationToken cancellationToken = default)
    {
        var venue = await QueryApprovedVenues()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (venue is null)
        {
            return null;
        }

        var dateOptions = Enumerable.Range(0, 7)
            .Select(offset => PresentationFormatter.FormatDateChip(DateTime.Today.AddDays(offset)))
            .ToList();

        return new VenueDetailViewModel
        {
            Venue = MapVenue(venue),
            TimeSlots = MockData.TimeSlots.ToList(),
            DateOptions = dateOptions,
            SlotMatrix = BuildSlotMatrix(venue, DateTime.Today)
        };
    }

    public async Task<Dictionary<string, Dictionary<string, SlotStatus>>?> GetSlotMatrixAsync(string id, string dateKey, CancellationToken cancellationToken = default)
    {
        var venue = await QueryApprovedVenues()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (venue is null)
        {
            return null;
        }

        var date = PresentationFormatter.TryParseDateChip(dateKey, out var parsedDate)
            ? parsedDate
            : DateTime.Today;

        return BuildSlotMatrix(venue, date);
    }

    private IQueryable<VenueEntity> QueryApprovedVenues()
    {
        return _context.Venues
            .AsNoTracking()
            .AsSplitQuery()
            .Where(venue => venue.Status == VenueStatus.Approved)
            .Include(venue => venue.Courts)
                .ThenInclude(court => court.Bookings);
    }

    private static Venue MapVenue(VenueEntity record)
    {
        var activeCourts = record.Courts
            .Where(court => court.IsActive)
            .OrderBy(court => court.Name)
            .ToList();

        return new Venue
        {
            Id = record.Id,
            Name = record.Name,
            District = record.District,
            Address = record.Address,
            OpenHours = record.OpenHours,
            OwnerPhone = record.OwnerPhone,
            Highlight = record.Highlight,
            Rating = record.Rating,
            Reviews = record.Reviews,
            ResponseFast = record.ResponseFast,
            HasSlotsToday = HasAvailableSlot(record, DateTime.Today),
            Description = record.Description,
            PriceFrom = activeCourts.Min(court => (int?)court.PricePerHour) ?? 0,
            Courts = activeCourts.Select(court => new Court
            {
                Id = court.Id,
                Name = court.Name,
                PricePerHour = court.PricePerHour,
                Note = court.Note
            }).ToList()
        };
    }

    private static List<Venue> SortVenues(IEnumerable<Venue> venues, VenueSearchCriteria criteria)
    {
        return criteria.Sort switch
        {
            "price-asc" => venues
                .OrderBy(venue => venue.PriceFrom)
                .ThenByDescending(venue => venue.Rating)
                .ThenBy(venue => venue.Name)
                .ToList(),
            "rating-desc" => venues
                .OrderByDescending(venue => venue.Rating)
                .ThenBy(venue => venue.PriceFrom)
                .ThenBy(venue => venue.Name)
                .ToList(),
            _ => SortByRelevance(venues, criteria.Query)
        };
    }

    private static List<Venue> SortByRelevance(IEnumerable<Venue> venues, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return venues
                .OrderByDescending(venue => venue.Rating)
                .ThenBy(venue => venue.Name)
                .ToList();
        }

        return venues
            .OrderBy(venue => venue.Name.StartsWith(query, StringComparison.CurrentCultureIgnoreCase) ? 0 : 1)
            .ThenBy(venue => venue.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase) ? 0 : 1)
            .ThenBy(venue => venue.District.Contains(query, StringComparison.CurrentCultureIgnoreCase) ? 0 : 1)
            .ThenByDescending(venue => venue.Rating)
            .ThenBy(venue => venue.Name)
            .ToList();
    }

    private static bool MatchesHoursFilter(string openHours, string hoursFilter)
    {
        if (!TryParseOpenHours(openHours, out var openTime, out var closeTime))
        {
            return false;
        }

        return hoursFilter switch
        {
            "morning" => openTime < TimeSpan.FromHours(6),
            "late" => closeTime > TimeSpan.FromHours(22),
            _ => true
        };
    }

    private static bool TryParseOpenHours(string openHours, out TimeSpan openTime, out TimeSpan closeTime)
    {
        openTime = default;
        closeTime = default;

        var parts = openHours.Split(" – ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 2
            && TimeSpan.TryParse(parts[0], CultureInfo.InvariantCulture, out openTime)
            && TimeSpan.TryParse(parts[1], CultureInfo.InvariantCulture, out closeTime);
    }

    private static bool HasAvailableSlot(VenueEntity venue, DateTime date)
    {
        var isToday = date.Date == DateTime.Today;

        return venue.Courts
            .Where(court => court.IsActive)
            .Any(court => SlotStarts.Any(slotStart =>
            {
                var startAt = date.Date.Add(slotStart);
                var endAt = startAt.AddHours(1);
                return ResolveSlotStatus(court.Bookings, startAt, endAt, isToday) == SlotStatus.Available;
            }));
    }

    private static Dictionary<string, Dictionary<string, SlotStatus>> BuildSlotMatrix(VenueEntity venue, DateTime date)
    {
        var matrix = new Dictionary<string, Dictionary<string, SlotStatus>>();
        var isToday = date.Date == DateTime.Today;

        foreach (var court in venue.Courts.Where(item => item.IsActive).OrderBy(item => item.Name))
        {
            var slots = new Dictionary<string, SlotStatus>();
            for (var index = 0; index < MockData.TimeSlots.Count; index++)
            {
                var slotLabel = MockData.TimeSlots[index];
                var slotStart = date.Date.Add(SlotStarts[index]);
                var slotEnd = slotStart.AddHours(1);

                slots[slotLabel] = ResolveSlotStatus(court.Bookings, slotStart, slotEnd, isToday);
            }

            matrix[court.Name] = slots;
        }

        return matrix;
    }

    private static SlotStatus ResolveSlotStatus(IEnumerable<BookingEntity> bookings, DateTime slotStart, DateTime slotEnd, bool blockPastSlots)
    {
        if (blockPastSlots && slotStart <= DateTime.Now)
        {
            return SlotStatus.Unavailable;
        }

        var overlappingBookings = bookings
            .Where(item => item.Status != BookingStatus.Cancelled
                && item.StartAt < slotEnd
                && slotStart < item.EndAt)
            .ToList();

        if (overlappingBookings.Count == 0)
        {
            return SlotStatus.Available;
        }

        return overlappingBookings.All(item => item.Status == BookingStatus.Pending)
            ? SlotStatus.Pending
            : SlotStatus.Booked;
    }
}
