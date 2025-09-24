using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace IdentityDemos.WebClient.Pages
{
    public class ClientCredsPoPModel(IHttpClientFactory httpClientFactory, IConfiguration configuration) : PageModel
    {
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            var client = httpClientFactory.CreateClient("DPoP-API");

            var response = await client.GetAsync("/api/echo-pop/Hello%20DPoP");

            string json = await response.Content.ReadAsStringAsync();
            ViewData["ApiResponse"] = JsonSerializer.Serialize(JsonSerializer.Deserialize<dynamic>(json), new JsonSerializerOptions { WriteIndented = true });

            var jwtHandler = new JwtSecurityTokenHandler();
            var dpopToken = response.RequestMessage?.Headers.ElementAtOrDefault(0).Value.FirstOrDefault();
            var token = jwtHandler.ReadJwtToken(dpopToken);
            ViewData["Token"] = JsonSerializer.Serialize(token.Claims.ToDictionary(c => c.Type, c => c.Value), new JsonSerializerOptions { WriteIndented = true });

            return Page();
        }
    }
}
