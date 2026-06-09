using System.Globalization;

namespace BadmintonCourtBooking.Services;

public static class PresentationFormatter
{
    private static readonly string[] ViDays =
    {
        "Chủ nhật",
        "Thứ 2",
        "Thứ 3",
        "Thứ 4",
        "Thứ 5",
        "Thứ 6",
        "Thứ 7"
    };

    public static string FormatDateChip(DateTime date)
    {
        if (date.Date == DateTime.Today)
        {
            return $"Hôm nay · {date:dd/MM}";
        }

        if (date.Date == DateTime.Today.AddDays(1))
        {
            return $"Ngày mai · {date:dd/MM}";
        }

        return $"{ViDays[(int)date.DayOfWeek]} · {date:dd/MM}";
    }

    public static bool TryParseDateChip(string dateKey, out DateTime date)
    {
        date = DateTime.Today;
        if (string.IsNullOrWhiteSpace(dateKey))
        {
            return false;
        }

        var token = dateKey.Split(" · ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).LastOrDefault();
        if (token is null)
        {
            return false;
        }

        if (!DateTime.TryParseExact(token, "dd/MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return false;
        }

        date = new DateTime(DateTime.Today.Year, parsed.Month, parsed.Day);
        return true;
    }

    public static string FormatBookingDate(DateTime date) => $"{ViDays[(int)date.DayOfWeek]}, {date:dd/MM/yyyy}";

    public static string FormatTimeRange(DateTime startAt, DateTime endAt) => $"{startAt:HH:mm} – {endAt:HH:mm}";

    public static string FormatJoinDate(DateTime date) => date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    public static string MaskPhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.Length < 7)
        {
            return phone;
        }

        return $"{digits[..4]}***{digits[^3..]}";
    }
}
