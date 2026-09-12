using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ZeldaArena.Web.Areas.Identity.Pages.Account.Manage;

[Authorize]
public sealed class ShowRecoveryCodesModel : PageModel
{
    public IReadOnlyList<string> Codes { get; private set; } = [];

    public IActionResult OnGet()
    {
        if (TempData["RecoveryCodes"] is not string codes || string.IsNullOrEmpty(codes))
        {
            return RedirectToPage("./TwoFactorAuthentication");
        }

        Codes = codes.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        return Page();
    }
}