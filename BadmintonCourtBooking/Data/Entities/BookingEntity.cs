using BadmintonCourtBooking.Models;

namespace BadmintonCourtBooking.Data.Entities;

public class BookingEntity
{
    public string Id { get; set; } = string.Empty;
    public string CourtId { get; set; } = string.Empty;
    public string? PlayerUserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int TotalPrice { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAt { get; set; }
    public string? CancelReason { get; set; }
    public AppUserEntity? PlayerUser { get; set; }
    public CourtEntity? Court { get; set; }
}
