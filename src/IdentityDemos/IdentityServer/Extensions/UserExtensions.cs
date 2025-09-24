using System.Security.Claims;

namespace IdentityServer.Extensions;

public static class UserExtensions
{
    public static bool AuthenticatedWithMfa(this ClaimsPrincipal user)
    {
        return user.Claims.Any(c => c is { Type: "amr", Value: "mfa" });
    }

    public static bool UserDeclinedMfa(this ClaimsPrincipal user)
    {
        return user.Claims.Any(c => c is { Type: "declined_mfa", Value: "true" });
    }
}