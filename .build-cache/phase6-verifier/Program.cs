using System.Security.Claims;
using BadmintonCourtBooking.Controllers;
using BadmintonCourtBooking.Data;
using BadmintonCourtBooking.Data.Entities;
using BadmintonCourtBooking.Models;
using BadmintonCourtBooking.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var appRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "BadmintonCourtBooking"));

var configuration = new ConfigurationBuilder()
    .SetBasePath(appRoot)
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing DefaultConnection.");

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlServer(connectionString)
    .Options;

await using var context = new ApplicationDbContext(options);
await using var transaction = await context.Database.BeginTransactionAsync();

var admin = await context.Users
    .AsNoTracking()
    .SingleAsync(user => user.Id == DemoDataConstants.DemoAdminUserId);

var adminService = new AdminDashboardService(context, BuildCurrentUserService(admin));
var venueCatalogService = new VenueCatalogService(context);
var passwordHasher = new PasswordHasher<AppUserEntity>();
var accountService = new AccountService(context, passwordHasher);

AssertAdminControllerProtected();

var initialDashboard = await adminService.GetDashboardAsync();
await AssertDashboardMatchesDbAsync(context, initialDashboard);

const string pendingVenueId = "pv1";

var publicVenuesBefore = await venueCatalogService.GetApprovedVenuesAsync();
Assert(!publicVenuesBefore.Any(venue => venue.Id == pendingVenueId), "Pending venue should not appear on public pages before approval.");

var approveResult = await adminService.ApproveVenueAsync(pendingVenueId);
Assert(approveResult.Succeeded, "Admin should approve a pending venue.");

var venueAfterApprove = await context.Venues
    .AsNoTracking()
    .SingleAsync(venue => venue.Id == pendingVenueId);

Assert(venueAfterApprove.Status == VenueStatus.Approved, "Approved venue should persist Approved status.");
Assert((await venueCatalogService.GetApprovedVenuesAsync()).Any(venue => venue.Id == pendingVenueId),
    "Approved venue should appear on public venue listing.");
Assert(await venueCatalogService.GetVenueDetailAsync(pendingVenueId) is not null,
    "Approved venue should have a public detail page.");

var rejectResult = await adminService.RejectVenueAsync(pendingVenueId);
Assert(rejectResult.Succeeded, "Admin should reject or hide a venue.");

var venueAfterReject = await context.Venues
    .AsNoTracking()
    .SingleAsync(venue => venue.Id == pendingVenueId);

Assert(venueAfterReject.Status == VenueStatus.Rejected, "Rejected venue should persist Rejected status.");
Assert(!(await venueCatalogService.GetApprovedVenuesAsync()).Any(venue => venue.Id == pendingVenueId),
    "Rejected venue should disappear from public venue listing.");
Assert(await venueCatalogService.GetVenueDetailAsync(pendingVenueId) is null,
    "Rejected venue should no longer have a public detail page.");

const string lockableUserId = "user-player-bao";

var lockResult = await adminService.LockUserAsync(lockableUserId);
Assert(lockResult.Succeeded, "Admin should be able to lock a non-admin account.");

var lockedUser = await context.Users
    .AsNoTracking()
    .SingleAsync(user => user.Id == lockableUserId);

Assert(!lockedUser.IsActive, "Locked user should persist IsActive=false.");

var lockedLogin = await accountService.LoginAsync(new LoginViewModel
{
    EmailOrPhone = lockedUser.Email,
    Password = "CourtBook@123",
    Role = "player"
});

Assert(!lockedLogin.Succeeded && lockedLogin.Message.Contains("bị khóa", StringComparison.OrdinalIgnoreCase),
    "Locked user should not be able to log in.");

await AssertFilterBlocksLockedUserAsync(context, lockedUser);

var lockAdminResult = await adminService.LockUserAsync(DemoDataConstants.DemoAdminUserId);
Assert(!lockAdminResult.Succeeded, "Admin account should not be lockable.");

var dashboardAfterLock = await adminService.GetDashboardAsync();
await AssertDashboardMatchesDbAsync(context, dashboardAfterLock);
Assert(dashboardAfterLock.LockedUsers == initialDashboard.LockedUsers + 1,
    "Locked user count should increase after locking an account.");

var unlockResult = await adminService.UnlockUserAsync(lockableUserId);
Assert(unlockResult.Succeeded, "Admin should be able to unlock a locked account.");

var unlockedUser = await context.Users
    .AsNoTracking()
    .SingleAsync(user => user.Id == lockableUserId);

Assert(unlockedUser.IsActive, "Unlocked user should persist IsActive=true.");

var unlockedLogin = await accountService.LoginAsync(new LoginViewModel
{
    EmailOrPhone = unlockedUser.Email,
    Password = "CourtBook@123",
    Role = "player"
});

Assert(unlockedLogin.Succeeded, "Unlocked user should be able to log in again.");

await AssertFilterAllowsActiveUserAsync(context, unlockedUser);

var finalDashboard = await adminService.GetDashboardAsync();
await AssertDashboardMatchesDbAsync(context, finalDashboard);

Console.WriteLine("Phase 6 verifier passed.");
Console.WriteLine($"Users={finalDashboard.TotalUsers}, PublicVenues={finalDashboard.ApprovedVenues}, LockedUsers={finalDashboard.LockedUsers}, Revenue={finalDashboard.ConfirmedRevenue}");

static async Task AssertDashboardMatchesDbAsync(ApplicationDbContext context, AdminDashboardViewModel dashboard)
{
    var users = await context.Users
        .AsNoTracking()
        .Where(user => user.Role != AppRoles.Admin)
        .OrderByDescending(user => user.JoinedAt)
        .ToListAsync();

    var venues = await context.Venues
        .AsNoTracking()
        .AsSplitQuery()
        .Include(venue => venue.Courts)
        .ToListAsync();

    var bookings = await context.Bookings
        .AsNoTracking()
        .AsSplitQuery()
        .Include(booking => booking.Court)
            .ThenInclude(court => court!.Venue)
        .OrderByDescending(booking => booking.StartAt)
        .ToListAsync();

    var bookingSummaries = bookings
        .Select(booking => new
        {
            Booking = booking,
            EffectiveStatus = ResolveBookingStatus(booking),
            VenueId = booking.Court?.VenueId ?? string.Empty
        })
        .ToList();

    var today = DateTime.Today;

    Assert(dashboard.TotalUsers == users.Count, "Dashboard total users should match DB.");
    Assert(dashboard.TotalOwners == users.Count(user => user.Role == AppRoles.Owner), "Dashboard owner count should match DB.");
    Assert(dashboard.TotalPlayers == users.Count(user => user.Role == AppRoles.Player), "Dashboard player count should match DB.");
    Assert(dashboard.LockedUsers == users.Count(user => !user.IsActive), "Dashboard locked user count should match DB.");
    Assert(dashboard.TotalVenues == venues.Count, "Dashboard total venue count should match DB.");
    Assert(dashboard.ApprovedVenues == venues.Count(venue => venue.Status == VenueStatus.Approved), "Dashboard approved venue count should match DB.");
    Assert(dashboard.PendingVenueCount == venues.Count(venue => venue.Status == VenueStatus.PendingApproval), "Dashboard pending venue count should match DB.");
    Assert(dashboard.RejectedVenues == venues.Count(venue => venue.Status == VenueStatus.Rejected), "Dashboard rejected venue count should match DB.");
    Assert(dashboard.TotalCourts == venues.Sum(venue => venue.Courts.Count), "Dashboard total court count should match DB.");
    Assert(dashboard.ActiveCourts == venues.Sum(venue => venue.Courts.Count(court => court.IsActive)), "Dashboard active court count should match DB.");
    Assert(dashboard.TotalBookings == bookings.Count, "Dashboard booking count should match DB.");
    Assert(dashboard.MonthlyBookings == bookingSummaries.Count(item =>
        item.EffectiveStatus != BookingStatus.Cancelled
        && item.Booking.StartAt.Month == today.Month
        && item.Booking.StartAt.Year == today.Year), "Dashboard monthly booking count should match DB.");
    Assert(dashboard.PendingBookings == bookingSummaries.Count(item => item.EffectiveStatus == BookingStatus.Pending), "Dashboard pending booking count should match DB.");
    Assert(dashboard.ConfirmedRevenue == bookingSummaries
        .Where(item => item.EffectiveStatus is BookingStatus.Confirmed or BookingStatus.Completed)
        .Sum(item => item.Booking.TotalPrice), "Dashboard revenue should match DB.");

    var expectedPendingVenueIds = venues
        .Where(venue => venue.Status == VenueStatus.PendingApproval)
        .OrderByDescending(venue => venue.CreatedAt)
        .Select(venue => venue.Id)
        .ToList();

    Assert(dashboard.PendingVenues.Select(venue => venue.Id).SequenceEqual(expectedPendingVenueIds),
        "Pending venue list should match DB ordering.");

    var expectedUserIds = users
        .OrderByDescending(user => user.JoinedAt)
        .Select(user => user.Id)
        .ToList();

    Assert(dashboard.Users.Select(user => user.Id).SequenceEqual(expectedUserIds),
        "User overview should include every non-admin account in DB order.");

    var expectedVenueIds = venues
        .OrderBy(venue => GetVenueSortOrder(venue.Status))
        .ThenByDescending(venue => venue.UpdatedAt)
        .Select(venue => venue.Id)
        .ToList();

    Assert(dashboard.Venues.Select(venue => venue.Id).SequenceEqual(expectedVenueIds),
        "Venue overview should reflect DB venue ordering.");

    var expectedRecentBookingIds = bookings
        .OrderByDescending(booking => booking.StartAt)
        .Take(12)
        .Select(booking => booking.Id)
        .ToList();

    Assert(dashboard.RecentBookings.Select(booking => booking.Id).SequenceEqual(expectedRecentBookingIds),
        "Recent booking overview should match DB ordering.");
}

static void AssertAdminControllerProtected()
{
    var attributes = typeof(AdminController)
        .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
        .OfType<AuthorizeAttribute>()
        .ToList();

    Assert(attributes.Any(attribute =>
        !string.IsNullOrWhiteSpace(attribute.Roles)
        && attribute.Roles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Contains(AppRoles.Admin)), "Admin controller should require the Admin role.");
}

static async Task AssertFilterBlocksLockedUserAsync(ApplicationDbContext context, AppUserEntity lockedUser)
{
    var filter = new RequireActiveUserFilter(context);
    var filterContext = BuildFilterContext(lockedUser);
    var actionExecuted = false;

    await filter.OnActionExecutionAsync(
        filterContext,
        () =>
        {
            actionExecuted = true;
            return Task.FromResult(new ActionExecutedContext(filterContext, new List<IFilterMetadata>(), new object()));
        });

    Assert(!actionExecuted, "Locked user filter should short-circuit MVC execution.");
    Assert(filterContext.Result is RedirectToActionResult redirect
        && redirect.ControllerName == "Account"
        && redirect.ActionName == "Login", "Locked user should be redirected to Account/Login.");
}

static async Task AssertFilterAllowsActiveUserAsync(ApplicationDbContext context, AppUserEntity activeUser)
{
    var filter = new RequireActiveUserFilter(context);
    var filterContext = BuildFilterContext(activeUser);
    var actionExecuted = false;

    await filter.OnActionExecutionAsync(
        filterContext,
        () =>
        {
            actionExecuted = true;
            return Task.FromResult(new ActionExecutedContext(filterContext, new List<IFilterMetadata>(), new object()));
        });

    Assert(actionExecuted, "Active user filter should allow MVC execution to continue.");
    Assert(filterContext.Result is null, "Active user filter should not override the action result.");
}

static ActionExecutingContext BuildFilterContext(AppUserEntity user)
{
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie();

    var provider = services.BuildServiceProvider();
    var httpContext = new DefaultHttpContext
    {
        RequestServices = provider,
        User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
            new Claim(ClaimTypes.Role, user.Role)
        ],
        CookieAuthenticationDefaults.AuthenticationScheme))
    };

    var actionContext = new ActionContext(
        httpContext,
        new RouteData(),
        new ActionDescriptor(),
        new ModelStateDictionary());

    return new ActionExecutingContext(
        actionContext,
        new List<IFilterMetadata>(),
        new Dictionary<string, object?>(),
        new object());
}

static BookingStatus ResolveBookingStatus(BookingEntity booking)
{
    return booking.Status != BookingStatus.Cancelled && booking.EndAt < DateTime.Now
        ? BookingStatus.Completed
        : booking.Status;
}

static int GetVenueSortOrder(VenueStatus status) => status switch
{
    VenueStatus.PendingApproval => 0,
    VenueStatus.Approved => 1,
    _ => 2
};

static FixedCurrentUserService BuildCurrentUserService(AppUserEntity user)
{
    return new FixedCurrentUserService(new CurrentUserInfo(
        user.Id,
        user.FullName,
        user.Role,
        user.Email,
        user.PhoneNumber));
}

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

file sealed class FixedCurrentUserService(CurrentUserInfo? user) : ICurrentUserService
{
    public bool IsAuthenticated => user is not null;
    public CurrentUserInfo? User => user;
}
