namespace BadmintonCourtBooking.Data.Entities;

public class CourtEntity
{
    public string Id { get; set; } = string.Empty;
    public string VenueId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int PricePerHour { get; set; }
    public string? Note { get; set; }
    public bool IsActive { get; set; } = true;
    public VenueEntity? Venue { get; set; }
    public ICollection<BookingEntity> Bookings { get; set; } = new List<BookingEntity>();
}
