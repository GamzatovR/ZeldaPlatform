using System.ComponentModel.DataAnnotations;

using ZeldaArena.Application.Common.Validation;

namespace ZeldaArena.Web.Models.Account;

public sealed class TwoFactorLoginViewModel
{
    [Required(ErrorMessage = "Введите код из приложения-аутентификатора.")]
    [StringLength(
        AccountValidationRules.TwoFactorCodeLength,
        MinimumLength = AccountValidationRules.TwoFactorCodeLength,
        ErrorMessage = "Код состоит из {1} цифр.")]
    [DataType(DataType.Text)]
    [Display(Name = "Код из приложения")]
    public string Code { get; set; } = string.Empty;

    public bool RememberMe { get; set; }

    [Display(Name = "Не спрашивать код на этом устройстве 30 дней")]
    public bool RememberDevice { get; set; }
}