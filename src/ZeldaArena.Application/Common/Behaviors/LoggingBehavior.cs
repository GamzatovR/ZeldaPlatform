using System.Diagnostics;

using MediatR;

using Microsoft.Extensions.Logging;

using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Application.Common.Behaviors;

/// <summary>
/// Самый внешний behavior: пишет начало, исход и длительность любого сценария
/// (docs/SPEC.md §13). Стоит снаружи валидации, поэтому в лог попадает и запрос,
/// отвергнутый валидатором.
///
/// Содержимое запроса здесь не пишется намеренно — только имя, пользователь и время.
/// Разбор полей с маскированием секретов делает <c>AuditBehavior</c>, и делает его
/// в одном месте.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    ICurrentUserService currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var requestName = typeof(TRequest).Name;
        var startedAt = Stopwatch.GetTimestamp();

        logger.LogInformation(
            "Сценарий {RequestName} начат пользователем {UserId}",
            requestName,
            currentUser.UserId);

        try
        {
            var response = await next(cancellationToken).ConfigureAwait(false);

            logger.LogInformation(
                "Сценарий {RequestName} завершён за {ElapsedMilliseconds} мс",
                requestName,
                Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);

            return response;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Сценарий {RequestName} упал через {ElapsedMilliseconds} мс",
                requestName,
                Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);

            throw;
        }
    }
}