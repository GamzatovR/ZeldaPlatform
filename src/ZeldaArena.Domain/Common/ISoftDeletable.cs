namespace ZeldaArena.Domain.Common;

/// <summary>Мягкое удаление.</summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; }

    DateTimeOffset? DeletedAt { get; }
}