using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityDemos.WebClient.Pages
{
    [Authorize]
    public class LoginModel : PageModel
    {
        public IActionResult OnGet()
        {
            var returnUrl = HttpUtility.UrlDecode(Request.Query["returnUrl"]);
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect("/");
        }
    }
}
