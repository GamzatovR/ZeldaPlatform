using System.ComponentModel.DataAnnotations;

namespace ZeldaArena.Web.Models.Billing;

public sealed class CardPaymentViewModel
{
    [Required(ErrorMessage = "Выберите тариф.")]
    public Guid PlanId { get; set; }

    /// <summary>Показывается на форме, чтобы человек видел, за что платит.</summary>
    public string? PlanName { get; set; }

    public decimal? Price { get; set; }

    public string? Currency { get; set; }

    public CardDetailsInputModel Card { get; set; } = new();

    [Required]
    public string IdempotencyKey { get; set; } = string.Empty;
}