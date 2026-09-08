using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ZeldaArena.Web.Areas.Identity.Pages.Account.Manage;

/// <summary>
/// Единственный показ кодов восстановления. Коды приходят через TempData: в адресной
/// строке они попали бы в историю браузера и в журналы веб-сервера (docs/SPEC.md §13).
///
/// Прямой заход без свежесгенерированных кодов возвращает на страницу 2FA: показывать
/// здесь нечего, а Identity хранит коды так, что прочитать их повторно нельзя.
/// </summary>
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