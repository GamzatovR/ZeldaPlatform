using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>
/// Реализация <see cref="IUserAdministrationService"/> поверх <see cref="UserManager{TUser}"/>.
/// Как и <see cref="IdentityUserAccountService"/>, решений не принимает: кого можно
/// блокировать и какие роли назначаются вручную, решают хендлеры Application.
/// </summary>
public sealed class IdentityUserAdministrationService(UserManager<ApplicationUser> userManager)
    : IUserAdministrationService
{
    public Task<int> CountUsersAsync(CancellationToken cancellationToken = default) =>
        userManager.Users.CountAsync(cancellationToken);
}