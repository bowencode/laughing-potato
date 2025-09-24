using Duende.IdentityModel;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace IdentityServer.Pages.Account.Mfa
{
    public class IndexModel(IIdentityServerInteractionService interaction) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public ViewModel? View { get; set; }

        public async Task OnGetAsync(string returnUrl)
        {
            await BuildModelAsync(returnUrl);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var existingProps = (await HttpContext.AuthenticateAsync()).Properties;

                var claims = Input.Button == "accept" ?
                    User.Claims
                        .Append(new Claim(JwtClaimTypes.AuthenticationMethod, "mfa"))
                        .Append(new Claim(JwtClaimTypes.AuthenticationContextClassReference, "1")) :
                    User.Claims
                        .Append(new Claim("declined_mfa", "true"));
                var newPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "mfa", "name", "role"));
                await HttpContext.SignInAsync(newPrincipal, existingProps);

                var authContext = await interaction.GetAuthorizationContextAsync(Input.ReturnUrl);
                if (authContext != null)
                {
                    // Safe to trust input, because authContext is non-null
                    return Redirect(Input.ReturnUrl!);
                }
            }
            // something went wrong, show form with error
            await BuildModelAsync(Input.ReturnUrl);
            return Page();
        }

        public async Task BuildModelAsync(string? returnUrl)
        {
            var context = await interaction.GetAuthorizationContextAsync(returnUrl);

            if (context != null)
            {
                Input = new InputModel
                {
                    ReturnUrl = returnUrl
                };

                View = new ViewModel
                {
                    ClientName = context.Client.ClientName,
                    MfaRequestedByClient = context.AcrValues.Contains("mfa")
                };
            }

        }
    }

    public class InputModel
    {
        public string? ReturnUrl { get; set; }

        public string? Button { get; set; }
    }

    public class ViewModel
    {
        public bool MfaRequestedByClient { get; set; }
        public string? ClientName { get; set; }
    }
}
