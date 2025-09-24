using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Home
{
    [AllowAnonymous]
    public class Index : PageModel
    {
    }
}
