using System.Security.Claims;
using BadmintonCourtBooking.Data.Entities;
using BadmintonCourtBooking.Extensions;
using BadmintonCourtBooking.Models;
using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ICurrentUserService _currentUserService;

        public AccountController(IAccountService accountService, ICurrentUserService currentUserService)
        {
            _accountService = accountService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string role = "player", string? returnUrl = null, bool locked = false)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectAfterAuthentication(returnUrl);
            }

            if (locked)
            {
                TempData["ToastMessage"] = "Tài khoản của bạn đang bị khóa. Vui lòng liên hệ quản trị viên CourtBook.";
                TempData["ToastType"] = "error";
            }

            var model = new LoginViewModel
            {
                Role = role,
                ReturnUrl = returnUrl
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                TempData["ToastMessage"] = ModelState.GetFirstErrorMessage("Vui lòng kiểm tra lại thông tin đăng nhập.");
                TempData["ToastType"] = "error";
                return View(model);
            }

            var result = await _accountService.LoginAsync(model, cancellationToken);
            if (!result.Succeeded || result.Data is null)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                TempData["ToastMessage"] = result.Message;
                TempData["ToastType"] = "error";
                return View(model);
            }

            await SignInAsync(result.Data, model.RememberMe);

            TempData["ToastMessage"] = result.Message;
            TempData["ToastType"] = "success";
            return RedirectAfterAuthentication(model.ReturnUrl, result.Data.Role);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string role = "player", string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectAfterAuthentication(returnUrl);
            }

            var model = new RegisterViewModel
            {
                Role = role,
                ReturnUrl = returnUrl
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                TempData["ToastMessage"] = ModelState.GetFirstErrorMessage("Vui lòng điền đầy đủ các thông tin cần thiết.");
                TempData["ToastType"] = "error";
                return View(model);
            }

            var result = await _accountService.RegisterAsync(model, cancellationToken);
            if (!result.Succeeded || result.Data is null)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                TempData["ToastMessage"] = result.Message;
                TempData["ToastType"] = "error";
                return View(model);
            }

            await SignInAsync(result.Data, true);

            TempData["ToastMessage"] = result.Message;
            TempData["ToastType"] = "success";
            return RedirectAfterAuthentication(model.ReturnUrl, result.Data.Role);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile(CancellationToken cancellationToken)
        {
            var currentUser = _currentUserService.User;
            if (currentUser is null)
            {
                return RedirectToAction(nameof(Login));
            }

            var model = await _accountService.GetProfileAsync(currentUser.UserId, cancellationToken);
            if (model is null)
            {
                return RedirectToAction(nameof(Login));
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model, CancellationToken cancellationToken)
        {
            var currentUser = _currentUserService.User;
            if (currentUser is null)
            {
                return RedirectToAction(nameof(Login));
            }

            if (!ModelState.IsValid)
            {
                await PopulateProfileMetadataAsync(currentUser.UserId, model, cancellationToken);
                TempData["ToastMessage"] = ModelState.GetFirstErrorMessage("Vui lòng nhập đầy đủ và đúng định dạng các thông tin cá nhân.");
                TempData["ToastType"] = "error";
                return View("Profile", model);
            }

            var result = await _accountService.UpdateProfileAsync(currentUser.UserId, model, cancellationToken);
            if (!result.Succeeded || result.Data is null)
            {
                await PopulateProfileMetadataAsync(currentUser.UserId, model, cancellationToken);
                TempData["ToastMessage"] = result.Message;
                TempData["ToastType"] = "error";
                return View("Profile", model);
            }

            await SignInAsync(result.Data, true);

            TempData["ToastMessage"] = result.Message;
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, CancellationToken cancellationToken)
        {
            var currentUser = _currentUserService.User;
            if (currentUser is null)
            {
                return RedirectToAction(nameof(Login));
            }

            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                TempData["ToastMessage"] = "Vui lòng điền đầy đủ thông tin mật khẩu.";
                TempData["ToastType"] = "error";
                return RedirectToAction(nameof(Profile));
            }

            var result = await _accountService.ChangePasswordAsync(currentUser.UserId, currentPassword, newPassword, cancellationToken);
            TempData["ToastMessage"] = result.Message;
            TempData["ToastType"] = result.Succeeded ? "success" : "error";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNotifications(bool receiveConfirm, bool receiveReminder, bool receivePromo, CancellationToken cancellationToken)
        {
            var currentUser = _currentUserService.User;
            if (currentUser is null)
            {
                return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn." });
            }

            var result = await _accountService.UpdateNotificationsAsync(currentUser.UserId, receiveConfirm, receiveReminder, receivePromo, cancellationToken);
            return Json(new { success = result.Succeeded, message = result.Message });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["ToastMessage"] = "Đã đăng xuất tài khoản thành công.";
            TempData["ToastType"] = "success";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task SignInAsync(AppUserEntity user, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.MobilePhone, user.PhoneNumber),
                new(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = rememberMe,
                    ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(14) : null
                });
        }

        private IActionResult RedirectAfterAuthentication(string? returnUrl, string? role = null)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            var resolvedRole = role ?? _currentUserService.User?.Role;
            return resolvedRole switch
            {
                AppRoles.Owner => RedirectToAction("Dashboard", "Owner"),
                AppRoles.Admin => RedirectToAction("Index", "Admin"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        private async Task PopulateProfileMetadataAsync(string userId, ProfileViewModel model, CancellationToken cancellationToken)
        {
            var profile = await _accountService.GetProfileAsync(userId, cancellationToken);
            if (profile is null)
            {
                return;
            }

            model.BookingCount = profile.BookingCount;
            model.FavoriteVenuesCount = profile.FavoriteVenuesCount;
            model.ShowUpRate = profile.ShowUpRate;
            model.JoinedDate = profile.JoinedDate;
            model.IsPhoneVerified = profile.IsPhoneVerified;
            model.ReceiveBookingConfirm = profile.ReceiveBookingConfirm;
            model.ReceivePlayReminder = profile.ReceivePlayReminder;
            model.ReceivePromo = profile.ReceivePromo;
        }
    }
}
