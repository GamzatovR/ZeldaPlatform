using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;

namespace ZeldaArena.Application.Features.Account.Queries.GetAccountProfile;

/// <summary>
/// Профиль текущего пользователя. По соглашению проекта запрос не оборачивается
/// в Result: у чтения нет ожидаемых бизнес-неудач, а отсутствие пользователя —
/// это null (см. IQuery&lt;T&gt;).
///
/// На практике null здесь не встречается: страницы кабинета закрыты [Authorize],
/// и без аутентификации до хендлера дело не доходит.
/// </summary>
public sealed class GetAccountProfileQueryHandler(
    ICurrentUserService currentUser,
    IUserAccountService userAccounts)
    : IRequestHandler<GetAccountProfileQuery, UserAccountDto?>
{
    public Task<UserAccountDto?> Handle(
        GetAccountProfileQuery request,
        CancellationToken cancellationToken) =>
        currentUser.UserId is { } userId
            ? userAccounts.FindByIdAsync(userId, cancellationToken)
            : Task.FromResult<UserAccountDto?>(null);
}