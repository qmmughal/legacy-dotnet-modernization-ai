using Microsoft.AspNetCore.Identity;

namespace Meridian.Data;

public sealed class PortalUser : IdentityUser
{
    /// <summary>Sales region; replaces Session["UserRegion"] from the Web Forms app.</summary>
    public string? Region { get; set; }
}
