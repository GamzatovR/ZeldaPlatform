using System.ComponentModel.DataAnnotations;

namespace ZeldaArena.Web.Models.Billing;

public sealed class ConfirmPaymentViewModel
{
    public Guid PaymentId { get; set; }

    [Required(ErrorMessage = "Введите код из письма.")]
    [RegularExpression("^[0-9]{6}$", ErrorMessage = "Код состоит из 6 цифр.")]
    [Display(Name = "Код из письма")]
    public string ConfirmationCode { get; set; } = string.Empty;

    public string MaskedEmail { get; set; } = string.Empty;

    public int AttemptsLeft { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public bool CanResendNow { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;
}