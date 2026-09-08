using System.ComponentModel.DataAnnotations;

namespace ZeldaArena.Web.Models.Account;

public sealed class RecoveryCodeLoginViewModel
{
    [Required(ErrorMessage = "Введите код восстановления.")]
    [DataType(DataType.Text)]
    [Display(Name = "Код восстановления")]
    public string RecoveryCode { get; set; } = string.Empty;
}