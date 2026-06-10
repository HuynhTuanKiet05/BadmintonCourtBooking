using BadmintonCourtBooking.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BadmintonCourtBooking.Areas.Identity.Pages.Account;

[Authorize]
public class LogoutModel(SignInManager<AppUserEntity> signInManager) : PageModel
{
    private readonly SignInManager<AppUserEntity> _signInManager = signInManager;

    public IActionResult OnGet()
    {
        return RedirectToAction("Index", "Home", new { area = string.Empty });
    }

    public async Task<IActionResult> OnPost()
    {
        await _signInManager.SignOutAsync();
        TempData["ToastMessage"] = "Đã đăng xuất tài khoản thành công.";
        TempData["ToastType"] = "success";
        return RedirectToAction("Index", "Home", new { area = string.Empty });
    }
}
