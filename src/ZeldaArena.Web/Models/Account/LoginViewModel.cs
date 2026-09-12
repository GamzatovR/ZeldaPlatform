using System.ComponentModel.DataAnnotations;

namespace ZeldaArena.Web.Models.Account;

/// <summary>
/// Клиентский уровень двухуровневой валидации (docs/SPEC.md §15): DataAnnotations
/// плюс jquery-validation-unobtrusive подсвечивают поля не отправляя форму.
/// Серверная проверка выполняется всегда — FluentValidation в ValidationBehavior.
///
/// Отдельный тип, а не команда из Application: доменные сущности и команды
/// из HTTP-запроса не биндятся (docs/CONVENTIONS.md).
/// </summary>
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