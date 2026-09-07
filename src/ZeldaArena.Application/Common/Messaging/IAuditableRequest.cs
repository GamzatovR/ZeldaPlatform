namespace ZeldaArena.Application.Common.Messaging;

/// <summary>
/// Помечает команду, которую нужно записать в аудит действий (docs/SPEC.md §13).
/// Запрос сам называет, над чем работает: иначе <c>AuditBehavior</c> угадывал бы
/// сущность по имени команды, и запись «кто, что, над чем» получалась бы неполной (§12).
/// </summary>
public interface IAuditableRequest
{
    /// <summary>Тип сущности, например <c>Match</c>.</summary>
    string AuditEntityType { get; }

    /// <summary>
    /// Идентификатор сущности. <c>null</c> допустим, когда сущность ещё создаётся
    /// и ключа на момент вызова нет.
    /// </summary>
    string? AuditEntityId { get; }
}