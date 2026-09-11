using System.ComponentModel.DataAnnotations;

namespace ZeldaArena.Web.Models.Billing;

/// <summary>
/// Форма оплаты подписки: выбранный тариф и реквизиты карты (<see cref="CardDetailsInputModel"/>).
/// </summary>
public sealed class CardPaymentViewModel
{
    [Required(ErrorMessage = "Выберите тариф.")]
    public Guid PlanId { get; set; }

    /// <summary>Показывается на форме, чтобы человек видел, за что платит.</summary>
    public string? PlanName { get; set; }

    public decimal? Price { get; set; }

    public string? Currency { get; set; }

    public CardDetailsInputModel Card { get; set; } = new();

    /// <summary>
    /// Ключ идемпотентности выдаётся формой и переживает её повторную отправку:
    /// обновлённая страница и второй клик не создают второй платёж (§7.6).
    /// </summary>
    [Required]
    public string IdempotencyKey { get; set; } = string.Empty;
}