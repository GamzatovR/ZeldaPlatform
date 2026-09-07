using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Common.Behaviors;

/// <summary>
/// Самый внутренний behavior: команда и её сохранение выполняются в одной транзакции.
/// Ограничение <see cref="ICommandBase"/> означает, что для запросов на чтение
/// открытая транзакция вообще не появляется.
///
/// Внутри этой транзакции работают и обработчики доменных событий: их рассылает
/// интерсептор после сохранения, но до коммита. Плата за это описана
/// в docs/adr/ADR-0004.
/// </summary>
public sealed class TransactionBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommandBase
{
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        return unitOfWork.ExecuteInTransactionAsync(token => next(token), cancellationToken);
    }
}