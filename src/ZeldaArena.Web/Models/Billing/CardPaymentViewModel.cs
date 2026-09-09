using System.ComponentModel.DataAnnotations;

namespace ZeldaArena.Web.Models.Billing;

/// <summary>
/// Реквизиты карты — клиентский уровень двухуровневой валидации (docs/SPEC.md §15).
/// Серверная проверка выполняется всегда: FluentValidation в ValidationBehavior,
/// и без JavaScript форма отвергается точно так же (§19).
///
/// Модель живёт один запрос: номер и CVV уходят в команду, оттуда в платёжный
/// провайдер и нигде не сохраняются (§7.6).
/// </summary>
public sealed class CardPaymentViewModel
{
    [Required(ErrorMessage = "Выберите тариф.")]
    public Guid PlanId { get; set; }

    /// <summary>Показывается на форме, чтобы человек видел, за что платит.</summary>
    public string? PlanName { get; set; }

    public decimal? Price { get; set; }

    public string? Currency { get; set; }

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
    [Display(Name = "Адрес для чека и кода подтверждения")]
    public string ConfirmationEmail { get; set; } = string.Empty;

    /// <summary>
    /// Ключ идемпотентности выдаётся формой и переживает её повторную отправку:
    /// обновлённая страница и второй клик не создают второй платёж (§7.6).
    /// </summary>
    [Required]
    public string IdempotencyKey { get; set; } = string.Empty;
}