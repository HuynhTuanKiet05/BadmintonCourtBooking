using BadmintonCourtBooking.Models;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account/Login
        [HttpGet]
        public IActionResult Login(string role = "player")
        {
            if (Request.Cookies.ContainsKey("Auth_User"))
            {
                var currentRole = Request.Cookies["Auth_Role"] ?? "player";
                return RedirectToAction(currentRole == "owner" ? "Dashboard" : "Index", currentRole == "owner" ? "Owner" : "Home");
            }

            var model = new LoginViewModel { Role = role };
            return View(model);
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ToastMessage"] = "Vui lòng kiểm tra lại thông tin đăng nhập.";
                TempData["ToastType"] = "error";
                return View(model);
            }

            // Giả lập xác thực thành công
            Response.Cookies.Append("Auth_User", "true");
            Response.Cookies.Append("Auth_Role", model.Role);
            Response.Cookies.Append("Auth_Name", model.Role == "owner" ? "Chủ sân CourtBook" : "Nguyễn Minh Khoa");

            TempData["ToastMessage"] = "Đăng nhập thành công. Chào mừng trở lại!";
            TempData["ToastType"] = "success";

            if (model.Role == "owner")
            {
                return RedirectToAction("Dashboard", "Owner");
            }
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register(string role = "player")
        {
            if (Request.Cookies.ContainsKey("Auth_User"))
            {
                var currentRole = Request.Cookies["Auth_Role"] ?? "player";
                return RedirectToAction(currentRole == "owner" ? "Dashboard" : "Index", currentRole == "owner" ? "Owner" : "Home");
            }

            var model = new RegisterViewModel { Role = role };
            return View(model);
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ToastMessage"] = "Vui lòng điền đầy đủ các thông tin cần thiết.";
                TempData["ToastType"] = "error";
                return View(model);
            }

            // Giả lập tạo tài khoản thành công
            Response.Cookies.Append("Auth_User", "true");
            Response.Cookies.Append("Auth_Role", model.Role);
            Response.Cookies.Append("Auth_Name", model.FullName);

            if (model.Role == "owner")
            {
                TempData["ToastMessage"] = "Đã tạo tài khoản chủ sân. Hãy đăng ký cụm sân đầu tiên!";
                TempData["ToastType"] = "success";
                return RedirectToAction("Venues", "Owner");
            }
            
            TempData["ToastMessage"] = "Tạo tài khoản thành công! Chào mừng bạn đến với CourtBook.";
            TempData["ToastType"] = "success";
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Profile
        [HttpGet]
        public IActionResult Profile()
        {
            // Trả về dữ liệu profile giả lập, lấy từ cookies nếu có
            var model = new ProfileViewModel();
            if (Request.Cookies.ContainsKey("Auth_Name"))
            {
                model.FullName = Request.Cookies["Auth_Name"] ?? "Nguyễn Minh Khoa";
            }
            
            return View(model);
        }

        // POST: Account/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(ProfileViewModel model)
        {
            // Chỉ kiểm tra các trường cơ bản liên quan đến thông tin cá nhân
            if (string.IsNullOrEmpty(model.FullName) || string.IsNullOrEmpty(model.Phone) || string.IsNullOrEmpty(model.Email))
            {
                TempData["ToastMessage"] = "Vui lòng nhập đầy đủ Họ tên, Số điện thoại và Email.";
                TempData["ToastType"] = "error";
                return View("Profile", model);
            }

            // Cập nhật Cookie tên hiển thị
            Response.Cookies.Append("Auth_Name", model.FullName);

            TempData["ToastMessage"] = "Đã cập nhật hồ sơ cá nhân thành công.";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Profile));
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string currentPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
            {
                TempData["ToastMessage"] = "Vui lòng điền đầy đủ thông tin mật khẩu.";
                TempData["ToastType"] = "error";
                return RedirectToAction(nameof(Profile));
            }

            if (newPassword.Length < 8)
            {
                TempData["ToastMessage"] = "Mật khẩu mới phải có ít nhất 8 ký tự.";
                TempData["ToastType"] = "error";
                return RedirectToAction(nameof(Profile));
            }

            TempData["ToastMessage"] = "Đã thay đổi mật khẩu tài khoản thành công.";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Profile));
        }

        // POST: Account/UpdateNotifications
        [HttpPost]
        public IActionResult UpdateNotifications(bool receiveConfirm, bool receiveReminder, bool receivePromo)
        {
            // Trả về JSON để phục vụ việc bật tắt switch qua Ajax
            return Json(new { success = true, message = "Đã cập nhật tùy chọn nhận thông báo." });
        }

        // GET: Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("Auth_User");
            Response.Cookies.Delete("Auth_Role");
            Response.Cookies.Delete("Auth_Name");

            TempData["ToastMessage"] = "Đã đăng xuất tài khoản thành công.";
            TempData["ToastType"] = "success";
            return RedirectToAction("Index", "Home");
        }
    }
}
