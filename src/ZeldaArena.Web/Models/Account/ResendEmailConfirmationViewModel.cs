using System.ComponentModel.DataAnnotations;

namespace ZeldaArena.Web.Models.Account;

public sealed class ResendEmailConfirmationViewModel
{
    [Required(ErrorMessage = "Укажите адрес электронной почты.")]
    [EmailAddress(ErrorMessage = "Адрес электронной почты указан неверно.")]
    [Display(Name = "Адрес электронной почты")]
    public string Email { get; set; } = string.Empty;
}