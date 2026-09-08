using System.ComponentModel.DataAnnotations;

namespace ZeldaArena.Web.Models.Account;

public sealed class ChangeEmailViewModel
{
    [Required(ErrorMessage = "Укажите адрес электронной почты.")]
    [EmailAddress(ErrorMessage = "Адрес электронной почты указан неверно.")]
    [Display(Name = "Новый адрес электронной почты")]
    public string NewEmail { get; set; } = string.Empty;
}