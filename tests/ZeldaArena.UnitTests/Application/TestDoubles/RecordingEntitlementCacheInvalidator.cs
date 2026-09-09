using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

/// <summary>
/// Запоминает сбросы кэша прав. Проверять их нужно по-настоящему: без сброса
/// пользователь до пяти минут после оплаты видел бы платную функцию закрытой,
/// а после снятия фичи с тарифа — наоборот, открытой (docs/SPEC.md §7.3).
/// </summary>
internal sealed class RecordingEntitlementCacheInvalidator : IEntitlementCacheInvalidator
{
    private readonly List<Guid> _users = [];

    public IReadOnlyList<Guid> InvalidatedUsers => _users;

    public int InvalidatedEverything { get; private set; }

    public Task InvalidateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _users.Add(userId);

        return Task.CompletedTask;
    }

    public Task InvalidateAllAsync(CancellationToken cancellationToken = default)
    {
        InvalidatedEverything++;

        return Task.CompletedTask;
    }
}