using System.ComponentModel.DataAnnotations;

namespace BadmintonCourtBooking.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email hoặc số điện thoại")]
        [StringLength(160, ErrorMessage = "Thông tin đăng nhập tối đa 160 ký tự")]
        [Display(Name = "Email hoặc số điện thoại")]
        public string EmailOrPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100, ErrorMessage = "Mật khẩu tối đa 100 ký tự.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; } = true;

        [Required(ErrorMessage = "Vui lòng chọn vai trò đăng nhập.")]
        [RegularExpression("^(player|owner)$", ErrorMessage = "Vai trò đăng nhập không hợp lệ.")]
        public string Role { get; set; } = "player"; // "player" hoặc "owner"

        public string? ReturnUrl { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(120, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ {2} đến {1} ký tự.")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        [StringLength(160, ErrorMessage = "Email tối đa 160 ký tự.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(30, ErrorMessage = "Số điện thoại tối đa 30 ký tự.")]
        [RegularExpression(@"^[0-9+\-\s()]{8,30}$", ErrorMessage = "Số điện thoại chỉ nên gồm số và các ký tự + - ( ).")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải từ {2} ký tự trở lên.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn vai trò tài khoản.")]
        [RegularExpression("^(player|owner)$", ErrorMessage = "Vai trò đăng ký không hợp lệ.")]
        public string Role { get; set; } = "player"; // "player" hoặc "owner"

        public string? ReturnUrl { get; set; }
    }

    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(120, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ {2} đến {1} ký tự.")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = "Nguyễn Minh Khoa";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(30, ErrorMessage = "Số điện thoại tối đa 30 ký tự.")]
        [RegularExpression(@"^[0-9+\-\s()]{8,30}$", ErrorMessage = "Số điện thoại chỉ nên gồm số và các ký tự + - ( ).")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = "0905 221 884";

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        [StringLength(160, ErrorMessage = "Email tối đa 160 ký tự.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = "khoa.nguyen@email.com";

        [StringLength(120, ErrorMessage = "Khu vực thường chơi tối đa 120 ký tự.")]
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
