using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BadmintonCourtBooking.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    public IActionResult OnGet(string? returnUrl = null)
    {
        return RedirectToAction("Register", "Account", new { area = string.Empty, returnUrl });
    }
}
