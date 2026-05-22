using System.ComponentModel.DataAnnotations;

namespace BadmintonCourtBooking.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email hoặc số điện thoại")]
        [Display(Name = "Email hoặc số điện thoại")]
        public string EmailOrPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; } = true;

        [Required]
        public string Role { get; set; } = "player"; // "player" hoặc "owner"
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải từ {2} ký tự trở lên.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "player"; // "player" hoặc "owner"
    }

    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = "Nguyễn Minh Khoa";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = "0905 221 884";

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = "khoa.nguyen@email.com";

        [Display(Name = "Khu vực thường chơi")]
        public string PlayArea { get; set; } = "Quận 11, TP.HCM";

        // Thống kê
        public int BookingCount { get; set; } = 24;
        public int FavoriteVenuesCount { get; set; } = 3;
        public int ShowUpRate { get; set; } = 96; // 96%
        public string JoinedDate { get; set; } = "03/2025";
        public bool IsPhoneVerified { get; set; } = true;

        // Bảo mật (Đổi mật khẩu)
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Mật khẩu mới phải từ {2} ký tự trở lên.", MinimumLength = 8)]
        [Display(Name = "Mật khẩu mới")]
        public string? NewPassword { get; set; }

        // Thông báo
        public bool ReceiveBookingConfirm { get; set; } = true;
        public bool ReceivePlayReminder { get; set; } = true;
        public bool ReceivePromo { get; set; } = false;
    }
}
