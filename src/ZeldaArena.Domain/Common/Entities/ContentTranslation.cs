using ZeldaArena.Domain.Common.Exceptions;

namespace ZeldaArena.Domain.Common.Entities;

/// <summary>
/// Универсальная таблица переводов контента из БД: названия турниров, заголовки
/// новостей, названия товаров. Новый язык не требует ни миграции, ни правки схемы —
/// достаточно добавить строки и код культуры в конфиг (docs/SPEC.md §5.4, EP-7).
/// </summary>
public class ContentTranslation : BaseEntity
{
    private ContentTranslation()
    {
    }

    /// <summary>Имя типа сущности: Tournament, NewsArticle, Product.</summary>
    public string EntityType { get; private set; } = null!;

    public Guid EntityId { get; private set; }

    /// <summary>Код культуры: ru, en.</summary>
    public string CultureCode { get; private set; } = null!;

    /// <summary>Имя переводимого поля: Name, Title, Description.</summary>
    public string FieldName { get; private set; } = null!;

    public string Value { get; private set; } = null!;

    public static ContentTranslation Create(
        string entityType,
        Guid entityId,
        string cultureCode,
        string fieldName,
        string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(cultureCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        InvariantViolationException.ThrowIf(
            entityId == Guid.Empty,
            "translation.entity_required",
            "Перевод должен относиться к конкретной записи.");

        return new ContentTranslation
        {
            EntityType = entityType.Trim(),
            EntityId = entityId,
            CultureCode = cultureCode.Trim().ToLowerInvariant(),
            FieldName = fieldName.Trim(),
            Value = value.Trim(),
        };
    }

    public void UpdateValue(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }
}
