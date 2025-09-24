using Duende.IdentityModel;
using Duende.IdentityServer;
using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.ResponseHandling;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;
using IdentityServer.Extensions;

namespace IdentityServer;

public class StepUpInteractionResponseGenerator(
    IdentityServerOptions options,
    IClock clock,
    ILogger<AuthorizeInteractionResponseGenerator> logger,
    IConsentService consent,
    IProfileService profile)
    : AuthorizeInteractionResponseGenerator(options, clock, logger, consent, profile)
{
    protected override async Task<InteractionResponse> ProcessLoginAsync(ValidatedAuthorizeRequest request)
    {
        var result = await base.ProcessLoginAsync(request);

        if (!result.IsLogin && !result.IsError)
        {
            ArgumentNullException.ThrowIfNull(request.Subject);

            if (MfaRequestedByClient(request) && !request.Subject.AuthenticatedWithMfa())
            {
                if (request.Subject.UserDeclinedMfa())
                {
                    result.Error = OidcConstants.AuthorizeErrors.UnmetAuthenticationRequirements;
                }
                else
                {
                    result.RedirectUrl = "/Account/Mfa";
                }
            }
        }
        return result;
    }

    private bool MfaRequestedByClient(ValidatedAuthorizeRequest request)
    {
        return request.AuthenticationContextReferenceClasses?.Contains("mfa") == true;
    }
}
