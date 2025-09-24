using Duende.IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace IdentityDemos.WebClient.Pages
{
    [Authorize]
    public class SecureModel(IHttpClientFactory httpClientFactory) : PageModel
    {
        public async Task OnGet()
        {
            var client = httpClientFactory.CreateClient("ApiService");
            string? token = await HttpContext.GetTokenAsync("access_token");
            if (token != null)
                client.SetBearerToken(token);

            var response = await client.GetAsync("/api/echo");
            if (!response.IsSuccessStatusCode)
            {
                ViewData["ApiResponse"] = "Unable to call API: " + response.StatusCode;
                return;
            }

            var result = JsonSerializer.Deserialize<dynamic>(await response.Content.ReadAsStringAsync());
            ViewData["ApiResponse"] = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
