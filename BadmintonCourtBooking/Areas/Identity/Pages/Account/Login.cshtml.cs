using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BadmintonCourtBooking.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    public IActionResult OnGet(string? returnUrl = null, bool locked = false)
    {
        return RedirectToAction("Login", "Account", new { area = string.Empty, returnUrl, locked });
    }
}
