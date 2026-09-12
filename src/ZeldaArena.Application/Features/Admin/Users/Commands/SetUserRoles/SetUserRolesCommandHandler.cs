using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Constants;

namespace ZeldaArena.Application.Features.Admin.Users.Commands.SetUserRoles;

/// <summary>
/// Роль <c>Premium</c> вручную не назначается: её выдают и снимают обработчики событий
/// подписки, и она существует только ради бейджа (docs/SPEC.md §7.4, §20 п. 2).
/// Выданная руками, она разошлась бы с действительным состоянием подписки.
///
/// Последнего администратора разжаловать нельзя — иначе в админку не войдёт никто.
/// </summary>
public sealed class SetUserRolesCommandHandler(
    IUserAdministrationService users,
    IUserAccountService accounts,
    ICurrentUserService currentUser)
    : IRequestHandler<SetUserRolesCommand, Result>
{
    public async Task<Result> Handle(SetUserRolesCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var account = await accounts.FindByIdAsync(request.UserId, cancellationToken);

        if (account is null)
        {
            return Result.Failure(AccountErrors.UserNotFound);
        }

        // Premium остаётся таким, каким его выдала подписка: форма его не присылает,
        // а если прислала — это подделанный запрос.
        var wasPremium = account.Roles.Contains(RoleNames.Premium, StringComparer.OrdinalIgnoreCase);
        var requested = request.Roles
            .Where(role => !string.Equals(role, RoleNames.Premium, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (requested.Exists(role => !RoleNames.All.Contains(role, StringComparer.OrdinalIgnoreCase)))
        {
            return Result.Failure(AccountErrors.RoleNotAssignable);
        }

        var losesAdmin = account.Roles.Contains(RoleNames.Admin, StringComparer.OrdinalIgnoreCase)
            && !requested.Contains(RoleNames.Admin, StringComparer.OrdinalIgnoreCase);

        if (losesAdmin && await users.CountInRoleAsync(RoleNames.Admin, cancellationToken) <= 1)
        {
            return Result.Failure(AccountErrors.LastAdministrator);
        }

        if (losesAdmin && request.UserId == currentUser.UserId)
        {
            return Result.Failure(AccountErrors.CannotDemoteSelf);
        }

        if (wasPremium)
        {
            requested.Add(RoleNames.Premium);
        }

        return await users.SetRolesAsync(request.UserId, requested, cancellationToken);
    }
}