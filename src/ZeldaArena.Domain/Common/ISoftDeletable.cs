namespace ZeldaArena.Domain.Common;

/// <summary>
/// Мягкое удаление. Записи с <see cref="IsDeleted"/> отсекаются глобальным
/// query filter в <c>AppDbContext</c> (docs/SPEC.md §6).
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; }

    DateTimeOffset? DeletedAt { get; }
}