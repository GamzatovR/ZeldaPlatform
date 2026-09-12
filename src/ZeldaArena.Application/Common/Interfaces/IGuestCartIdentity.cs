namespace ZeldaArena.Application.Common.Interfaces;

public interface IGuestCartIdentity
{
    /// <summary>Проверенный идентификатор гостя или <c>null</c>, если куки нет или она подделана.</summary>
    Guid? AnonymousId { get; }
}