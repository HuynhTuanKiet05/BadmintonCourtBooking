using BadmintonCourtBooking.Models;

namespace BadmintonCourtBooking.Data.Entities;

public class AppUserEntity
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string NormalizedPhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = AppRoles.Player;
    public string PlayArea { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsPhoneVerified { get; set; } = true;
    public bool ReceiveBookingConfirm { get; set; } = true;
    public bool ReceivePlayReminder { get; set; } = true;
    public bool ReceivePromo { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSignInAt { get; set; }
    public ICollection<BookingEntity> PlayerBookings { get; set; } = new List<BookingEntity>();
}
