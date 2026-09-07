using FluentValidation;

using MediatR;

namespace ZeldaArena.Application.Common.Behaviors;

/// <summary>
/// Серверная половина двухуровневой валидации (docs/SPEC.md §15). Работает до хендлера,
/// поэтому бизнес-правила проверяются даже тогда, когда клиентская валидация отключена
/// вместе с JavaScript.
///
/// Валидаторы запускаются все сразу: пользователь должен увидеть полный список ошибок
/// формы, а не по одной за отправку. Ошибки уходят исключением
/// <see cref="ValidationException"/>, которое в Фазе 11 превращается в ModelState
/// для MVC и в ProblemDetails для API (§14.1).
/// </summary>
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
            return await next().ConfigureAwait(false);
        }

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            applicable.Select(validator => validator.ValidateAsync(context, cancellationToken)))
            .ConfigureAwait(false);

        var failures = results
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToArray();

        if (failures.Length > 0)
        {
            throw new ValidationException(failures);
        }

        return await next().ConfigureAwait(false);
    }
}