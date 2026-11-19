using System.Security.Claims;
using Meridian.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Meridian.Web.Identity;

/// <summary>Adds the sales region as a claim (replaces Session["UserRegion"]).</summary>
public sealed class PortalClaimsFactory(
    UserManager<PortalUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<PortalUser, IdentityRole>(userManager, roleManager, options)
{
    public const string RegionClaim = "meridian:region";

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(PortalUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var identity = await base.GenerateClaimsAsync(user);
        if (!string.IsNullOrEmpty(user.Region))
        {
            identity.AddClaim(new Claim(RegionClaim, user.Region));
        }

        return identity;
    }
}
