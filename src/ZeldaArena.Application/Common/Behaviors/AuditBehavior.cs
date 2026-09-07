using System.Diagnostics;

using MediatR;

using Microsoft.Extensions.Logging;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Behaviors;

/// <summary>
/// Аудит действий пользователя (docs/SPEC.md §13): кто, что, над чем, с какого адреса
/// и чем кончилось. Пишутся только команды, помеченные <see cref="IAuditableRequest"/>.
///
/// Стоит снаружи транзакции сознательно. Во-первых, аудит уезжает в MongoDB (§12)
/// и в транзакции PostgreSQL не участвует — попытка объединить их дала бы ложное
/// ощущение атомарности. Во-вторых, запись обязана появиться и при неуспехе: попытка
/// сделать то, на что нет прав, для разбора инцидента интереснее удавшегося действия.
///
/// Сбой самой записи аудита команду не роняет: пользователь не должен получать ошибку
/// из-за недоступного журнала.
/// </summary>
public sealed class AuditBehavior<TRequest, TResponse>(
    IAuditLogWriter auditLogWriter,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    ILogger<AuditBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IAuditableRequest
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            var response = await next(cancellationToken).ConfigureAwait(false);

            await WriteAsync(request, startedAt, Outcome(response), cancellationToken)
                .ConfigureAwait(false);

            return response;
        }
        catch (Exception exception)
        {
            await WriteAsync(request, startedAt, (false, exception.Message), cancellationToken)
                .ConfigureAwait(false);

            throw;
        }
    }

    /// <summary>
    /// Команда возвращает <see cref="Result"/>, поэтому отказ по бизнес-правилу виден
    /// без исключения — в аудит он попадает как неуспех с кодом ошибки.
    /// </summary>
    private static (bool Succeeded, string? FailureReason) Outcome(TResponse response) =>
        response is Result { IsFailure: true } result
            ? (false, result.Error.Code)
            : (true, null);

    private async Task WriteAsync(
        TRequest request,
        long startedAt,
        (bool Succeeded, string? FailureReason) outcome,
        CancellationToken cancellationToken)
    {
        var entry = new AuditLogEntry
        {
            Action = typeof(TRequest).Name,
            OccurredAt = dateTimeProvider.UtcNow,
            Succeeded = outcome.Succeeded,
            FailureReason = outcome.FailureReason,
            DurationMs = (long)Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds,
            UserId = currentUser.UserId,
            UserName = currentUser.UserName,
            EntityType = request.AuditEntityType,
            EntityId = request.AuditEntityId,
            PayloadJson = RequestPayloadSerializer.ToJson(request),
            IpAddress = currentUser.IpAddress,
            UserAgent = currentUser.UserAgent,
            CorrelationId = currentUser.CorrelationId,
        };

        try
        {
            await auditLogWriter.WriteAsync(entry, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Не удалось записать аудит действия {Action}", entry.Action);
        }
    }
}