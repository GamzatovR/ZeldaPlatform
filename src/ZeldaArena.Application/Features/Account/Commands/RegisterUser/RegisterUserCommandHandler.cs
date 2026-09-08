using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Account.Commands.RegisterUser;

/// <summary>
/// Заводит учётную запись и отправляет письмо со ссылкой подтверждения.
///
/// Занятость адреса здесь не проверяется отдельным запросом: уникальность обеспечена
/// индексом в базе, и Identity вернёт DuplicateEmail. Предварительная проверка была бы
/// вторым источником истины и всё равно оставляла бы гонку между проверкой и вставкой.
///
/// Письмо отправляется внутри транзакции команды намеренно. Если SMTP недоступен,
/// исключение откатит и создание пользователя: полурегистрация, при которой учётная
/// запись есть, а войти в неё нельзя и письмо не придёт, — худший из возможных исходов.
/// Пользователь просто повторит регистрацию.
/// </summary>
public sealed class RegisterUserCommandHandler(
    IUserAccountService userAccounts,
    IAccountEmailSender emailSender)
    : IRequestHandler<RegisterUserCommand, Result>
{
    public async Task<Result> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var created = await userAccounts
            .CreateAsync(
                request.Email,
                request.Password,
                request.DisplayName,
                request.PreferredCulture,
                cancellationToken)
            .ConfigureAwait(false);

        if (created.IsFailure)
        {
            return Result.Failure(created.Error);
        }

        var token = await userAccounts
            .GenerateEmailConfirmationTokenAsync(created.Value, cancellationToken)
            .ConfigureAwait(false);

        if (token.IsFailure)
        {
            return Result.Failure(token.Error);
        }

        await emailSender
            .SendEmailConfirmationAsync(
                request.Email,
                request.DisplayName,
                created.Value,
                token.Value,
                cancellationToken)
            .ConfigureAwait(false);

        return Result.Success();
    }
}