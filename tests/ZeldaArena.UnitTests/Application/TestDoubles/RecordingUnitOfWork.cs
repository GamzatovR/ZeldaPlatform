using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

/// <summary>
/// Единица работы, повторяющая семантику настоящей: успех коммитит, исключение
/// откатывает. Реализация, а не мок, потому что проверяется именно поведение
/// границы транзакции, а не факт вызова метода.
/// </summary>
internal sealed class RecordingUnitOfWork : IUnitOfWork
{
    public bool Committed { get; private set; }

    public bool RolledBack { get; private set; }

    public int SaveChangesCalls { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCalls++;

        return Task.FromResult(0);
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await operation(cancellationToken);

            Committed = true;

            return result;
        }
        catch
        {
            RolledBack = true;

            throw;
        }
    }
}