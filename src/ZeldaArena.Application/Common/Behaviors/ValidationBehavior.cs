using FluentValidation;

using MediatR;

namespace ZeldaArena.Application.Common.Behaviors;

/// <summary>Серверная половина двухуровневой валидации.</summary>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var applicable = validators as IValidator<TRequest>[] ?? validators.ToArray();

        if (applicable.Length == 0)
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }

        // Контекст создаётся свой на каждый валидатор.
        var results = await Task.WhenAll(
            applicable.Select(validator => validator.ValidateAsync(
                new ValidationContext<TRequest>(request),
                cancellationToken)))
            .ConfigureAwait(false);

        var failures = results
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToArray();

        if (failures.Length > 0)
        {
            throw new ValidationException(failures);
        }

        return await next(cancellationToken).ConfigureAwait(false);
    }
}