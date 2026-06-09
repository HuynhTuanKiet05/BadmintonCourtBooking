namespace BadmintonCourtBooking.Models;

public static class AppRoles
{
    public const string Player = "Player";
    public const string Admin = "Admin";

    public static string FromSelection(string? selection) => selection?.Trim().ToLowerInvariant() switch
    {
        "admin" => Admin,
        _ => Player
    };

    public static string ToSelection(string? role) => role switch
    {
        Admin => "admin",
        _ => "player"
    };

    public static string ToDisplayLabel(string? role) => role switch
    {
        Admin => "Quản trị viên",
        _ => "Khách hàng"
    };
}
