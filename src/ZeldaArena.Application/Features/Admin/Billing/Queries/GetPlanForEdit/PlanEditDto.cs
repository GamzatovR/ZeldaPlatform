namespace ZeldaArena.Application.Features.Admin.Billing.Queries.GetPlanForEdit;

public sealed record PlanEditDto
{
    public Guid Id { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public decimal Price { get; init; }

    public string Currency { get; init; } = string.Empty;

    public int DurationDays { get; init; }

    public int SortOrder { get; init; }

    public bool IsActive { get; init; }

    public int ActiveSubscriptions { get; init; }

    /// <summary>Все фичи системы с отметкой, входят ли они в этот тариф (EP-3, EP-4).</summary>
    public IReadOnlyList<PlanFeatureOptionDto> Features { get; init; } = [];

    /// <summary>Тариф с подписками не удаляется: подписка ссылается на него (docs/adr/ADR-0010).</summary>
    public bool CanDelete => ActiveSubscriptions == 0;
}