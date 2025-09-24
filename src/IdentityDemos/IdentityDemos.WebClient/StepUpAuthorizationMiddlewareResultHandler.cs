using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization.Policy;

namespace IdentityDemos.WebClient;

public class StepUpAuthorizationMiddlewareResultHandler
    : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _default = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authResult)
    {
        // If the authorization was forbidden due to a step-up requirement
        // challenge the user again to log back in passing requirements to IdentityServer
        if (authResult.Forbidden)
        {
            var failures = authResult
                .AuthorizationFailure!
                .FailedRequirements
                .ToList();

            var mfaRequirement = failures.OfType<ClaimsAuthorizationRequirement>()
                .FirstOrDefault(r => r.ClaimType == "amr" && r.AllowedValues!.Contains("mfa"));

            if (mfaRequirement is not null)
            {
                AuthenticationProperties props = new();
                props.Items.Add("acr_values", "mfa");

                await context.ChallengeAsync("oidc", props);
                // make sure to end here or else the default implementation will run
                return;
            }
        }

        // Fall back to the default implementation.
        await _default.HandleAsync(next, context, policy, authResult);
    }
}