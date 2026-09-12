using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;
using ZeldaArena.Infrastructure.Persistence.Ef;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>
/// Реализация <see cref="IUserAdministrationService"/>. Как и
/// <see cref="IdentityUserAccountService"/>, решений не принимает: кого можно
/// блокировать и какие роли назначаются вручную, решают хендлеры Application.
/// </summary>
public sealed class IdentityUserAdministrationService(
    UserManager<ApplicationUser> userManager,
    AppDbContext dbContext)
    : IUserAdministrationService
{
    public Task<int> CountUsersAsync(CancellationToken cancellationToken = default) =>
        userManager.Users.CountAsync(cancellationToken);

    public async Task<PagedResult<AdminUserRowDto>> SearchAsync(
        AdminUserFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var query = userManager.Users.AsNoTracking();

        if (filter.IsBlocked is { } blocked)
        {
            query = query.Where(user => user.IsBlocked == blocked);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search.Trim()}%";

            query = query.Where(user => EF.Functions.ILike(user.Email!, pattern)
                || (user.DisplayName != null && EF.Functions.ILike(user.DisplayName, pattern)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Role))
        {
            // Роли живут в связке AspNetUserRoles; соединение через контекст, а не
            // через UserManager.GetUsersInRoleAsync, который поднимает всех в память.
            var role = filter.Role;

            query = from user in query
                    join userRole in dbContext.UserRoles on user.Id equals userRole.UserId
                    join roleRow in dbContext.Roles on userRole.RoleId equals roleRow.Id
                    where roleRow.Name == role
                    select user;
        }

        query = filter.NewestFirst
            ? query.OrderByDescending(user => user.CreatedAt)
            : query.OrderBy(user => user.Email);

        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var page = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(user => new
            {
                user.Id,
                user.Email,
                user.DisplayName,
                user.EmailConfirmed,
                user.TwoFactorEnabled,
                user.IsBlocked,
                user.CreatedAt,
                user.LastLoginAt,
                Roles = (from userRole in dbContext.UserRoles
                         join roleRow in dbContext.Roles on userRole.RoleId equals roleRow.Id
                         where userRole.UserId == user.Id
                         select roleRow.Name!).ToList(),
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var rows = page
            .Select(user => new AdminUserRowDto(
                user.Id,
                user.Email ?? string.Empty,
                user.DisplayName,
                user.EmailConfirmed,
                user.TwoFactorEnabled,
                user.IsBlocked,
                user.CreatedAt,
                user.LastLoginAt,
                user.Roles))
            .ToList();

        return new PagedResult<AdminUserRowDto>(rows, filter.Page, filter.PageSize, total);
    }

    public async Task<Result> SetBlockedAsync(
        Guid userId,
        bool isBlocked,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString()).ConfigureAwait(false);

        if (user is null)
        {
            return Result.Failure(AccountErrors.UserNotFound);
        }

        user.IsBlocked = isBlocked;

        var updated = await userManager.UpdateAsync(user).ConfigureAwait(false);

        if (!updated.Succeeded)
        {
            return Result.Failure(AccountErrors.OperationFailed);
        }

        // Действующие cookie обесцениваются: вход уже закрыт проверкой IsBlocked,
        // но открытая сессия жила бы до истечения срока.
        var stamped = await userManager.UpdateSecurityStampAsync(user).ConfigureAwait(false);

        return stamped.Succeeded ? Result.Success() : Result.Failure(AccountErrors.OperationFailed);
    }

    public async Task<Result> SetRolesAsync(
        Guid userId,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(roles);
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString()).ConfigureAwait(false);

        if (user is null)
        {
            return Result.Failure(AccountErrors.UserNotFound);
        }

        var current = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        var removed = current.Except(roles, StringComparer.OrdinalIgnoreCase).ToArray();
        var added = roles.Except(current, StringComparer.OrdinalIgnoreCase).ToArray();

        if (removed.Length > 0
            && !(await userManager.RemoveFromRolesAsync(user, removed).ConfigureAwait(false)).Succeeded)
        {
            return Result.Failure(AccountErrors.OperationFailed);
        }

        if (added.Length > 0
            && !(await userManager.AddToRolesAsync(user, added).ConfigureAwait(false)).Succeeded)
        {
            return Result.Failure(AccountErrors.OperationFailed);
        }

        // Роли лежат в cookie снимком на момент входа (Фаза 4): без смены стампа
        // новая роль появилась бы у пользователя только через пять минут.
        var stamped = await userManager.UpdateSecurityStampAsync(user).ConfigureAwait(false);

        return stamped.Succeeded ? Result.Success() : Result.Failure(AccountErrors.OperationFailed);
    }

    public Task<int> CountInRoleAsync(string role, CancellationToken cancellationToken = default) =>
        (from userRole in dbContext.UserRoles
         join roleRow in dbContext.Roles on userRole.RoleId equals roleRow.Id
         where roleRow.Name == role
         select userRole.UserId)
        .CountAsync(cancellationToken);
}