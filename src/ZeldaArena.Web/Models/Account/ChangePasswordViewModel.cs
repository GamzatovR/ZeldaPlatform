using System.ComponentModel.DataAnnotations;

using ZeldaArena.Domain.Constants;

namespace ZeldaArena.Web.Models.Account;

public sealed class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Укажите текущий пароль.")]
    [DataType(DataType.Password)]
    [Display(Name = "Текущий пароль")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите пароль.")]
    [StringLength(
        PasswordPolicy.MaximumLength,
        MinimumLength = PasswordPolicy.MinimumLength,
        ErrorMessage = "Пароль должен быть от {2} до {1} символов.")]
    [DataType(DataType.Password)]
    [Display(Name = "Новый пароль")]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Пароли не совпадают.")]
    [Display(Name = "Подтверждение нового пароля")]
    public string ConfirmPassword { get; set; } = string.Empty;
}