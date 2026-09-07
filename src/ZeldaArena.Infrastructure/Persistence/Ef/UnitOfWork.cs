using Microsoft.EntityFrameworkCore;

using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Infrastructure.Persistence.Ef;

/// <summary>
/// Граница транзакции поверх <see cref="AppDbContext"/>.
/// </summary>
public sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        // Транзакция уже открыта выше по стеку — присоединяемся к ней. Вторая
        // транзакция на том же контексте невозможна, а хендлер, вызванный из другого
        // хендлера, должен работать.
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