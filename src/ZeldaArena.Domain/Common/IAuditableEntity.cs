namespace ZeldaArena.Domain.Common;

/// <summary>Сущность с отметками времени.</summary>
public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; }

    DateTimeOffset? UpdatedAt { get; }
}