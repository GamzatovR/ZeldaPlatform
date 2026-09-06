using ZeldaArena.Domain.Common;

namespace ZeldaArena.Domain.Billing;

/// <summary>
/// Платная функция. Ключ всей системы доступа: код никогда не спрашивает,
/// какой у пользователя тариф, — только есть ли у него фича (docs/SPEC.md §8.1).
/// Новая фича добавляется строкой в этой таблице через админку, без правки кода (EP-3).
/// </summary>
public class Feature : BaseEntity
{
    private readonly List<PlanFeature> _planFeatures = [];

    private Feature()
    {
    }

    /// <summary>Код вида team.create — то, что проверяет IEntitlementService.</summary>
    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<PlanFeature> PlanFeatures => _planFeatures.AsReadOnly();

    public static Feature Create(string code, string name, string? description = null, bool isActive = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Feature
        {
            Code = code.Trim().ToLowerInvariant(),
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            IsActive = isActive,
        };
    }

    public void UpdateDetails(string name, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
