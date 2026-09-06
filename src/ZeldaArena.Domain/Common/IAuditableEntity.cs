namespace ZeldaArena.Domain.Common;

/// <summary>
/// Сущность с отметками времени. Сеттеров нет намеренно: значения проставляет
/// EF-интерсептор через метаданные (<c>entry.Property(...).CurrentValue</c>),
/// и правило «нет публичных сеттеров» остаётся ненарушенным.
/// </summary>
public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; }

    DateTimeOffset? UpdatedAt { get; }
}