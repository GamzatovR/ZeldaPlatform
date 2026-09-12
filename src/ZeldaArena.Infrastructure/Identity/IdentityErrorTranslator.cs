using Microsoft.AspNetCore.Identity;

using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>Переводит IdentityResult в общий для решения Result.</summary>
internal static class IdentityErrorTranslator
{
    public static Result ToResult(IdentityResult identityResult) =>
        identityResult.Succeeded
            ? Result.Success()
            : Result.Failure(ToError(identityResult));

    public static Result<TValue> ToResult<TValue>(IdentityResult identityResult, TValue value) =>
        identityResult.Succeeded
            ? Result.Success(value)
            : Result.Failure<TValue>(ToError(identityResult));

    public static Error ToError(IdentityResult identityResult)
    {
        var first = identityResult.Errors.FirstOrDefault();

        if (first is null)
        {
            return AccountErrors.OperationFailed;
        }

        var known = Map(first.Code);

        // Код — наш ключ ресурса, описание — оригинальное, для логов и отладки.
        return new Error(known.Code, first.Description);
    }

    private static Error Map(string identityErrorCode) => identityErrorCode switch
    {
        nameof(IdentityErrorDescriber.DuplicateEmail)
            or nameof(IdentityErrorDescriber.DuplicateUserName) => AccountErrors.EmailAlreadyTaken,

        nameof(IdentityErrorDescriber.PasswordMismatch) => AccountErrors.IncorrectPassword,

        nameof(IdentityErrorDescriber.InvalidToken) => AccountErrors.InvalidToken,

        nameof(IdentityErrorDescriber.RecoveryCodeRedemptionFailed) =>
            AccountErrors.RecoveryCodeInvalid,

        // Все требования к паролю — одна ошибка формы: показывать их по одной значит
        // заставлять подбирать пароль в несколько заходов.
        nameof(IdentityErrorDescriber.PasswordTooShort)
            or nameof(IdentityErrorDescriber.PasswordRequiresDigit)
            or nameof(IdentityErrorDescriber.PasswordRequiresLower)
            or nameof(IdentityErrorDescriber.PasswordRequiresUpper)
            or nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric)
            or nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars) =>
            AccountErrors.PasswordRejected,

        _ => AccountErrors.OperationFailed,
    };
}