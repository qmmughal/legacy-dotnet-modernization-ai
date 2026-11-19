using Meridian.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Web.Endpoints;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        // Form posts from the static-SSR Login page. Minimal API form binding
        // validates the antiforgery token because UseAntiforgery() is in the pipeline.
        app.MapPost("/account/login", async (
            [FromForm] string userName,
            [FromForm] string password,
            [FromForm] string? returnUrl,
            // Unchecked HTML checkboxes are not posted at all (Web Forms' CheckBox.Checked
            // hid this), so the flag must be optional.
            [FromForm] bool? rememberMe,
            SignInManager<PortalUser> signIn) =>
        {
            var result = await signIn.PasswordSignInAsync(userName, password, rememberMe ?? false, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                return Results.Redirect("/account/login?error=1");
            }

            // Only local return URLs: the Web Forms app used RedirectFromLoginPage,
            // which also refused off-site URLs. Keep that protection.
            return Results.LocalRedirect(IsLocal(returnUrl) ? returnUrl! : "/orders");
        });

        app.MapPost("/account/logout", async (SignInManager<PortalUser> signIn) =>
        {
            await signIn.SignOutAsync();
            return Results.LocalRedirect("/account/login");
        }).RequireAuthorization();

        return app;
    }

    private static bool IsLocal(string? url) =>
        !string.IsNullOrEmpty(url) && url[0] == '/' && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'));
}
