using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ZeldaArena.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public sealed class LockoutModel : PageModel
{
    public void OnGet()
    {
    }
}