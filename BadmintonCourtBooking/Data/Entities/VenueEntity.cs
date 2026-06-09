using BadmintonCourtBooking.Models;

namespace BadmintonCourtBooking.Data.Entities;

public class VenueEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string OpenHours { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Highlight { get; set; }
    public double Rating { get; set; }
    public int Reviews { get; set; }
    public bool ResponseFast { get; set; }
    public bool HasSlotsToday { get; set; }
    public VenueStatus Status { get; set; } = VenueStatus.Approved;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<CourtEntity> Courts { get; set; } = new List<CourtEntity>();
}
