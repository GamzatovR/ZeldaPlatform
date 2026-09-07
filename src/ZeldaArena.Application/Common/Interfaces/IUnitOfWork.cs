namespace ZeldaArena.Application.Common.Interfaces;

/// <summary>
/// Граница транзакции. Транзакция наружу объектом не выносится: <c>TransactionBehavior</c>
/// не должен знать, что под ним <c>IDbContextTransaction</c>, — иначе EF Core протёк бы
/// в Application (docs/SPEC.md §5.2, правило 2).
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Выполняет операцию в транзакции: успех коммитит, исключение откатывает.
    /// Вложенный вызов не открывает вторую транзакцию, а присоединяется к текущей —
    /// иначе хендлер, вызванный из другого хендлера, ронял бы сохранение.
    /// </summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default);
}