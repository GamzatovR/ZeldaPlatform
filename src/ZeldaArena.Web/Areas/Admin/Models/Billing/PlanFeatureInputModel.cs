namespace ZeldaArena.Web.Areas.Admin.Models.Billing;

/// <summary>
/// Строка набора фич тарифа: отметка и параметр (EP-5). Форма присылает все фичи,
/// а команда получает только отмеченные — так снятая галочка действительно снимает
/// привязку, а не просто не добавляет её.
/// </summary>
public sealed class PlanFeatureInputModel
{
    public Guid FeatureId { get; set; }

    public bool IsGranted { get; set; }

    public string? Value { get; set; }
}