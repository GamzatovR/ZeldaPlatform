using System.ComponentModel.DataAnnotations;

using ZeldaArena.Application.Common.Validation;
using ZeldaArena.Application.Features.Payments;

namespace ZeldaArena.Web.Models.Billing;

public sealed class CardDetailsInputModel : IValidatableObject
{
    [Required(ErrorMessage = "Укажите номер карты.")]
    [CreditCard(ErrorMessage = "Номер карты указан неверно.")]
    [DataType(DataType.CreditCard)]
    [Display(Name = "Номер карты")]
    public string CardNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите месяц.")]
    [Range(1, 12, ErrorMessage = "Месяц — число от 1 до 12.")]
    [Display(Name = "Месяц")]
    public int ExpiryMonth { get; set; }

    [Required(ErrorMessage = "Укажите год.")]
    [Range(2000, 2100, ErrorMessage = "Год указан неверно.")]
    [Display(Name = "Год")]
    public int ExpiryYear { get; set; }

    [Required(ErrorMessage = "Укажите CVV.")]
    [RegularExpression("^[0-9]{3}$", ErrorMessage = "CVV состоит из 3 цифр.")]
    [DataType(DataType.Password)]
    [Display(Name = "CVV")]
    public string Cvv { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите адрес для чека.")]
    [EmailAddress(ErrorMessage = "Адрес электронной почты указан неверно.")]
    [StringLength(AccountValidationRules.MaxEmailLength, ErrorMessage = "Адрес не длиннее {1} символов.")]
    [Display(Name = "Адрес для чека и кода подтверждения")]
    public string ConfirmationEmail { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CardPaymentRules.IsExpired(ExpiryMonth, ExpiryYear))
        {
            yield return new ValidationResult("Срок действия карты истёк.", [nameof(ExpiryYear)]);
        }
    }
}