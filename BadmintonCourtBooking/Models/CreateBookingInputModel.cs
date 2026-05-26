using System.ComponentModel.DataAnnotations;

namespace BadmintonCourtBooking.Models;

public class CreateBookingInputModel
{
    [Required(ErrorMessage = "Thiếu thông tin cụm sân cần đặt.")]
    [StringLength(64, ErrorMessage = "Mã cụm sân không hợp lệ.")]
    public string VenueId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn sân con trước khi đặt.")]
    [StringLength(64, ErrorMessage = "Mã sân con không hợp lệ.")]
    public string CourtId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn ngày chơi.")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Ngày chơi phải theo định dạng yyyy-MM-dd.")]
    public string BookingDate { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn khung giờ chơi.")]
    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Giờ bắt đầu phải theo định dạng HH:mm.")]
    public string StartTime { get; set; } = string.Empty;
}
