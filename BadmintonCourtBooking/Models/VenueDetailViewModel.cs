namespace BadmintonCourtBooking.Models;

public class VenueDetailViewModel
{
    public Venue Venue { get; set; } = null!;
    public Dictionary<string, Dictionary<string, SlotStatus>> SlotMatrix { get; set; } = new();
    public List<string> TimeSlots { get; set; } = new();
    public List<string> DateOptions { get; set; } = new();
}
