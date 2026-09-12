using Microsoft.EntityFrameworkCore;

using ZeldaArena.Application.Common.Exceptions;
using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Infrastructure.Persistence.Ef;

public sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConcurrencyConflictException(exception.Message, exception);
        }
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        // Транзакция уже открыта выше по стеку — присоединяемся к ней.
        if (context.Database.CurrentTransaction is not null)
        {
            return await operation(cancellationToken).ConfigureAwait(false);
        }

        // Стратегия выполнения, а не голый BeginTransaction: если в конфигурации
        // включатся повторы при сбое соединения, транзакция должна повторяться целиком.
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(
            cancellationToken,
            async (token) =>
            {
                await using var transaction = await context.Database
                    .BeginTransactionAsync(token)
                    .ConfigureAwait(false);

                var result = await operation(token).ConfigureAwait(false);

                await transaction.CommitAsync(token).ConfigureAwait(false);

                return result;
            }).ConfigureAwait(false);
    }
}