using ZeldaArena.Application.Common.Exceptions;
using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

internal sealed class RecordingUnitOfWork : IUnitOfWork
{
    public bool Committed { get; private set; }

    public bool RolledBack { get; private set; }

    public int SaveChangesCalls { get; private set; }

    /// <summary>Следующее сохранение упадёт на конфликте токена — как при гонке за последней единицей.</summary>
    public bool ConflictOnNextSave { get; set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCalls++;

        if (ConflictOnNextSave)
        {
            ConflictOnNextSave = false;
            throw new ConcurrencyConflictException();
        }

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