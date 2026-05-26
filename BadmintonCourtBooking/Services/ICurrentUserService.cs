namespace BadmintonCourtBooking.Services;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    CurrentUserInfo? User { get; }
}

public sealed record CurrentUserInfo(
    string UserId,
    string FullName,
    string Role,
    string Email,
    string PhoneNumber);
