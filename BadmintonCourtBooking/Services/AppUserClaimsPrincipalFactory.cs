using System.Security.Claims;
using BadmintonCourtBooking.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BadmintonCourtBooking.Services;

public class AppUserClaimsPrincipalFactory(
    UserManager<AppUserEntity> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> optionsAccessor)
    : UserClaimsPrincipalFactory<AppUserEntity, IdentityRole>(userManager, roleManager, optionsAccessor)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUserEntity user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        foreach (var claim in identity.FindAll(ClaimTypes.Name).ToList())
        {
            identity.RemoveClaim(claim);
        }

        identity.AddClaim(new Claim(ClaimTypes.Name, user.FullName));

        if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            identity.AddClaim(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
        }

        return identity;
    }
}
