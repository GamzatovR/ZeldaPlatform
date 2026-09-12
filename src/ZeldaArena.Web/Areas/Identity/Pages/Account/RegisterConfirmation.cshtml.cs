using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ZeldaArena.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public sealed class RegisterConfirmationModel : PageModel
{
    public string? Email { get; private set; }

    public void OnGet(string? email) => Email = email;
}