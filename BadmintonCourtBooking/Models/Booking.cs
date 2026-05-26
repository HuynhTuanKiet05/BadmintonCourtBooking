namespace BadmintonCourtBooking.Models;

public enum BookingStatus { Confirmed, Pending, Completed, Cancelled }
public enum SlotStatus { Available, Booked, Pending, Unavailable }

public class Booking
{
    public string Id { get; set; } = string.Empty;
    public string VenueId { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Court { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public int Total { get; set; }
    public BookingStatus Status { get; set; }
    public bool CanCancel { get; set; }
    public string When { get; set; } = string.Empty; // upcoming, completed, cancelled
}

public class OwnerBooking
{
    public string Id { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Court { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public int Total { get; set; }
    public BookingStatus Status { get; set; }
}
