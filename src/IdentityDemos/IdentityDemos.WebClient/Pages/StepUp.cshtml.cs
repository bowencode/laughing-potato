using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace IdentityDemos.WebClient.Pages
{
    [Authorize("RequireMfa")]
    public class StepUpModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
