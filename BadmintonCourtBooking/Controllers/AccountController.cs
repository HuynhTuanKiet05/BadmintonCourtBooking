using System.Security.Claims;
using BadmintonCourtBooking.Data.Entities;
using BadmintonCourtBooking.Extensions;
using BadmintonCourtBooking.Models;
using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonCourtBooking.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ICurrentUserService _currentUserService;
        private readonly SignInManager<AppUserEntity> _signInManager;
        private readonly UserManager<AppUserEntity> _userManager;

        public AccountController(
            IAccountService accountService,
            ICurrentUserService currentUserService,
            SignInManager<AppUserEntity> signInManager,
            UserManager<AppUserEntity> userManager)
        {
            _accountService = accountService;
            _currentUserService = currentUserService;
            _signInManager = signInManager;
            _userManager = userManager;
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
                TempData["ToastMessage"] = "Tài khoản của bạn đang bị khóa. Vui lòng liên hệ quản trị viên Đặt Sân Cầu Lông.";
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

            await _signInManager.SignInAsync(result.Data, model.RememberMe);

            TempData["ToastMessage"] = result.Message;
            TempData["ToastType"] = "success";
            return await RedirectAfterAuthenticationAsync(model.ReturnUrl, result.Data);
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

            await _signInManager.SignInAsync(result.Data, isPersistent: true);

            TempData["ToastMessage"] = result.Message;
            TempData["ToastType"] = "success";
            return await RedirectAfterAuthenticationAsync(model.ReturnUrl, result.Data);
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

            await _signInManager.RefreshSignInAsync(result.Data);

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
            await _signInManager.SignOutAsync();
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

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info is null)
            {
                TempData["ToastMessage"] = "Đăng nhập bằng mạng xã hội không thành công hoặc đã bị hủy.";
                TempData["ToastType"] = "error";
                return RedirectToAction(nameof(Login));
            }

            var linkedSignInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: true,
                bypassTwoFactor: true);

            if (linkedSignInResult.Succeeded)
            {
                var linkedUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (linkedUser is null || !linkedUser.IsActive)
                {
                    await _signInManager.SignOutAsync();
                    TempData["ToastMessage"] = "Tài khoản của bạn đang bị khóa. Vui lòng liên hệ quản trị viên.";
                    TempData["ToastType"] = "error";
                    return RedirectToAction(nameof(Login));
                }

                linkedUser.LastSignInAt = DateTime.UtcNow;
                linkedUser.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(linkedUser);

                TempData["ToastMessage"] = $"Đăng nhập thành công! Chào mừng {linkedUser.FullName}!";
                TempData["ToastType"] = "success";
                return await RedirectAfterAuthenticationAsync(returnUrl, linkedUser);
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? "Người dùng mạng xã hội";

            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["ToastMessage"] = "Không thể lấy thông tin Email từ tài khoản của bạn.";
                TempData["ToastType"] = "error";
                return RedirectToAction(nameof(Login));
            }

            var userResult = await _accountService.GetOrCreateExternalUserAsync(email, name, cancellationToken);
            if (!userResult.Succeeded || userResult.Data == null)
            {
                TempData["ToastMessage"] = userResult.Message;
                TempData["ToastType"] = "error";
                return RedirectToAction(nameof(Login));
            }

            var user = userResult.Data;
            if (!user.IsActive)
            {
                TempData["ToastMessage"] = "Tài khoản của bạn đang bị khóa. Vui lòng liên hệ quản trị viên.";
                TempData["ToastType"] = "error";
                return RedirectToAction(nameof(Login));
            }

            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
            {
                var alreadyLinked = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (alreadyLinked?.Id != user.Id)
                {
                    TempData["ToastMessage"] = "Tài khoản mạng xã hội này đã được liên kết với người dùng khác.";
                    TempData["ToastType"] = "error";
                    return RedirectToAction(nameof(Login));
                }
            }

            await _signInManager.SignInAsync(user, isPersistent: true);

            TempData["ToastMessage"] = $"Đăng nhập thành công! Chào mừng {user.FullName}!";
            TempData["ToastType"] = "success";

            return await RedirectAfterAuthenticationAsync(returnUrl, user);
        }

        private IActionResult RedirectAfterAuthentication(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return User.IsInRole(AppRoles.Admin)
                ? RedirectToAction("Index", "Admin")
                : RedirectToAction("Index", "Home");
        }

        private async Task<IActionResult> RedirectAfterAuthenticationAsync(string? returnUrl, AppUserEntity user)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return await _userManager.IsInRoleAsync(user, AppRoles.Admin)
                ? RedirectToAction("Index", "Admin")
                : RedirectToAction("Index", "Home");
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
