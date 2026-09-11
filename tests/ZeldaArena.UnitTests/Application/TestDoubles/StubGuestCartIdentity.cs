using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

/// <summary>Гостевая кука, уже проверенная middleware: в тестах просто задаётся.</summary>
internal sealed class StubGuestCartIdentity : IGuestCartIdentity
{
    public Guid? AnonymousId { get; set; }
}