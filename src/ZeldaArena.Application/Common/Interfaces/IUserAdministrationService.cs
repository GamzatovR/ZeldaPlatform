using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Interfaces;

public interface IUserAdministrationService
{
    Task<int> CountUsersAsync(CancellationToken cancellationToken = default);

    Task<PagedResult<AdminUserRowDto>> SearchAsync(
        AdminUserFilter filter,
        CancellationToken cancellationToken = default);

    Task<Result> SetBlockedAsync(Guid userId, bool isBlocked, CancellationToken cancellationToken = default);

    Task<Result> SetRolesAsync(Guid userId, IReadOnlyCollection<string> roles, CancellationToken cancellationToken = default);

    /// <summary>Сколько всего пользователей с этой ролью — чтобы не разжаловать последнего администратора.</summary>
    Task<int> CountInRoleAsync(string role, CancellationToken cancellationToken = default);
}