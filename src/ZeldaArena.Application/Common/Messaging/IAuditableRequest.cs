namespace ZeldaArena.Application.Common.Messaging;

public interface IAuditableRequest
{
    /// <summary>Тип сущности, например <c>Match</c>.</summary>
    string AuditEntityType { get; }

    string? AuditEntityId { get; }
}