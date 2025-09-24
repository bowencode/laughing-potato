using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace IdentityDemos.WebClient.Pages
{
    public class ClientCredsJwtSecretModel(IHttpClientFactory httpClientFactory, IConfiguration configuration) : PageModel
    {
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            var client = httpClientFactory.CreateClient("IdentityServer");

            var credentials = new SigningCredentials(new JsonWebKey(SecurityExtensions.RsaJwk), SecurityAlgorithms.RsaSha256);

            var identityUrl = configuration.GetValue<string>(Program.IdentityUrlConfig);
            var response = await client.RequestSignedCredentialTokenAsync(identityUrl, credentials, "m2m.client.jwt", "scope1");

            if (response.IsError)
            {
                ModelState.AddModelError("error", response.Error ?? "Login failed");
                return Page();
            }

            ViewData["TokenResponse"] = JsonSerializer.Serialize(response.Json, new JsonSerializerOptions { WriteIndented = true });

            var jwtHandler = new JwtSecurityTokenHandler();
            var token = jwtHandler.ReadJwtToken(response.AccessToken);
            ViewData["Claims"] = JsonSerializer.Serialize(token.Claims.ToDictionary(c => c.Type, c => c.Value), new JsonSerializerOptions { WriteIndented = true });

            return Page();
        }
    }
}
