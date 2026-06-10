using System.Security.Claims;
using BadmintonCourtBooking.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Services;

public class RequireActiveUserFilter(ApplicationDbContext context) : IAsyncActionFilter
{
    private readonly ApplicationDbContext _context = context;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var principal = context.HttpContext.User;
        if (principal.Identity?.IsAuthenticated != true)
        {
            await next();
            return;
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            await ForceSignOutAsync(context);
            context.Result = BuildLockedResult(context);
            return;
        }

        var isActive = await _context.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => (bool?)user.IsActive)
            .FirstOrDefaultAsync(context.HttpContext.RequestAborted);

        if (isActive != true)
        {
            await ForceSignOutAsync(context);
            context.Result = BuildLockedResult(context);
            return;
        }

        await next();
    }

    private static async Task ForceSignOutAsync(FilterContext context)
    {
        await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
    }

    private static IActionResult BuildLockedResult(FilterContext context)
    {
        var acceptHeader = context.HttpContext.Request.Headers.Accept.ToString();
        var requestedWith = context.HttpContext.Request.Headers["X-Requested-With"].ToString();
        var acceptsJson = acceptHeader.Contains("application/json", StringComparison.OrdinalIgnoreCase)
            || string.Equals(requestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

        if (acceptsJson)
        {
            return new JsonResult(new
            {
                success = false,
                message = "Tài khoản của bạn đang bị khóa. Vui lòng liên hệ quản trị viên Đặt Sân Cầu Lông."
            })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }

        return new RedirectToPageResult("/Account/Login", new { area = "Identity", locked = true });
    }
}
