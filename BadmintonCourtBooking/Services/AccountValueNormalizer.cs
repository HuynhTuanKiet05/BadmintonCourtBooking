namespace BadmintonCourtBooking.Services;

public static class AccountValueNormalizer
{
    public static string NormalizeEmail(string? email) => (email ?? string.Empty).Trim().ToUpperInvariant();

    public static string NormalizePhone(string? phone) =>
        new string((phone ?? string.Empty).Where(char.IsDigit).ToArray());
}
