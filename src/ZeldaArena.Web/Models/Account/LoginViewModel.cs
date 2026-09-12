using System.ComponentModel.DataAnnotations;

namespace ZeldaArena.Web.Models.Account;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Укажите адрес электронной почты.")]
    [EmailAddress(ErrorMessage = "Адрес электронной почты указан неверно.")]
    [Display(Name = "Адрес электронной почты")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите пароль.")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Запомнить меня")]
    public bool RememberMe { get; set; }
}