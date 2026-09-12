using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Common.Validation;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Web.Areas.Api.Controllers;

namespace ZeldaArena.Web.Models.Account;

public sealed class RegisterViewModel
{
    [Required(ErrorMessage = "Укажите адрес электронной почты.")]
    [StringLength(AccountValidationRules.MaxEmailLength, ErrorMessage = "Адрес не длиннее {1} символов.")]
    [EmailAddress(ErrorMessage = "Адрес электронной почты указан неверно.")]
    [Remote("CheckEmail", "AccountApi", ApiControllerBase.AreaName,
        ErrorMessage = "Этот адрес электронной почты уже занят.")]
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