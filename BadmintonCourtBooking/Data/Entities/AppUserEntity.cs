using Microsoft.AspNetCore.Identity;

namespace BadmintonCourtBooking.Data.Entities;

public class AppUserEntity : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string PlayArea { get; set; } = string.Empty;
    public string? AvatarPath { get; set; }
    public bool IsActive { get; set; } = true;
    public bool ReceiveBookingConfirm { get; set; } = true;
    public bool ReceivePlayReminder { get; set; } = true;
    public bool ReceivePromo { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSignInAt { get; set; }
    public ICollection<BookingEntity> PlayerBookings { get; set; } = new List<BookingEntity>();
}
