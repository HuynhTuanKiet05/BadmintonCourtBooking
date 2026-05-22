namespace BadmintonCourtBooking.Models;

public class Court
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int PricePerHour { get; set; }
    public string? Note { get; set; }
}
