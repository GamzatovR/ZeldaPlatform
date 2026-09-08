using System.ComponentModel.DataAnnotations;

using ZeldaArena.Domain.Constants;

namespace ZeldaArena.Web.Models.Account;

/// <summary>
/// Форма регистрации. Требования к паролю продублированы здесь для подсказки
/// в браузере; источник истины — PasswordPolicy и FluentValidation на сервере.
/// </summary>
public sealed class RegisterViewModel
{
    [Required(ErrorMessage = "Укажите адрес электронной почты.")]
    [EmailAddress(ErrorMessage = "Адрес электронной почты указан неверно.")]
    [Display(Name = "Адрес электронной почты")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите пароль.")]
    [StringLength(
        PasswordPolicy.MaximumLength,
        MinimumLength = PasswordPolicy.MinimumLength,
        ErrorMessage = "Пароль должен быть от {2} до {1} символов.")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают.")]
    [Display(Name = "Подтверждение пароля")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [StringLength(64, ErrorMessage = "Отображаемое имя не длиннее {1} символов.")]
    [Display(Name = "Отображаемое имя")]
    public string? DisplayName { get; set; }
}