using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Domain.Billing;

/// <summary>
/// Тариф — это просто набор фич плюс цена и срок. Прав сам по себе не даёт:
/// доступ определяется фичами, поэтому новый тариф не требует ни строчки кода
/// (docs/SPEC.md §8.1, §8.2).
/// </summary>
public class Plan : BaseEntity, IAuditableEntity
{
    private readonly List<PlanFeature> _planFeatures = [];

    private Plan()
    {
    }

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public Money Price { get; private set; } = null!;

    /// <summary>Срок действия в днях. Ноль — бессрочный бесплатный тариф.</summary>
    public int DurationDays { get; private set; }

    public bool IsActive { get; private set; }

    public int SortOrder { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public IReadOnlyCollection<PlanFeature> PlanFeatures => _planFeatures.AsReadOnly();

    public bool IsFree => Price.IsZero;

    public static Plan Create(
        string code,
        string name,
        Money price,
        int durationDays,
        string? description = null,
        int sortOrder = 0,
        bool isActive = true)
    {
        ArgumentNullException.ThrowIfNull(price);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        InvariantViolationException.ThrowIf(
            durationDays < 0,
            "plan.negative_duration",
            "Срок действия тарифа не может быть отрицательным.");

        InvariantViolationException.ThrowIf(
            durationDays == 0 && !price.IsZero,
            "plan.paid_without_duration",
            "У платного тарифа обязан быть срок действия.");

        return new Plan
        {
            Code = code.Trim().ToLowerInvariant(),
            Name = name.Trim(),
            Price = price,
            DurationDays = durationDays,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            SortOrder = sortOrder,
            IsActive = isActive,
        };
    }

    public void UpdateDetails(string name, string? description, int sortOrder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        SortOrder = sortOrder;
    }

    public void ChangePrice(Money price, int durationDays)
    {
        ArgumentNullException.ThrowIfNull(price);

        InvariantViolationException.ThrowIf(
            durationDays < 0,
            "plan.negative_duration",
            "Срок действия тарифа не может быть отрицательным.");

        Price = price;
        DurationDays = durationDays;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    /// <summary>Привязывает фичу к тарифу. Повторная привязка обновляет параметр, а не дублирует строку.</summary>
    public PlanFeature GrantFeature(Guid featureId, string? value = null)
    {
        InvariantViolationException.ThrowIf(
            featureId == Guid.Empty,
            "plan.feature_required",
            "Для привязки нужна фича.");

        var existing = _planFeatures.SingleOrDefault(item => item.FeatureId == featureId);
        if (existing is not null)
        {
            existing.SetValue(value);
            return existing;
        }

        var planFeature = PlanFeature.Create(Id, featureId, value);
        _planFeatures.Add(planFeature);
        return planFeature;
    }

    /// <summary>Снимает фичу с тарифа — основной жест демонстрации EP-4.</summary>
    public void RevokeFeature(Guid featureId)
    {
        var planFeature = _planFeatures.SingleOrDefault(item => item.FeatureId == featureId);

        InvariantViolationException.ThrowIf(
            planFeature is null,
            "plan.feature_not_granted",
            "Эта фича к тарифу не привязана.");

        _planFeatures.Remove(planFeature!);
    }
}