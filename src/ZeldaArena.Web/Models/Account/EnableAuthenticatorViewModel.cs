using System.ComponentModel.DataAnnotations;

using ZeldaArena.Application.Common.Validation;

namespace ZeldaArena.Web.Models.Account;

public sealed class EnableAuthenticatorViewModel
{
    [Required(ErrorMessage = "Введите код из приложения-аутентификатора.")]
    [StringLength(
        AccountValidationRules.TwoFactorCodeLength,
        MinimumLength = AccountValidationRules.TwoFactorCodeLength,
        ErrorMessage = "Код состоит из {1} цифр.")]
    [DataType(DataType.Text)]
    [Display(Name = "Код из приложения")]
    public string VerificationCode { get; set; } = string.Empty;
}