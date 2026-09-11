using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

/// <summary>
/// Учётные записи для админских сценариев. Живёт отдельно от
/// <see cref="InMemoryUserAccountService"/> по той же причине, по которой разделены
/// сами порты (ISP): сценариям самообслуживания эти методы не нужны.
/// </summary>
internal sealed class InMemoryUserAdministrationService : IUserAdministrationService
{
    public int UserCount { get; set; }

    public Task<int> CountUsersAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(UserCount);
}