using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BadmintonCourtBooking.Areas.Identity.Pages.Account.Manage;

[Authorize]
public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        return RedirectToAction("Profile", "Account", new { area = string.Empty });
    }
}
