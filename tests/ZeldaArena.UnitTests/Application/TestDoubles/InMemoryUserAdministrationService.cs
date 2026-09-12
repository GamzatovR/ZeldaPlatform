using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

/// <summary>
/// Учётные записи для админских сценариев. Живёт отдельно от
/// <see cref="InMemoryUserAccountService"/> по той же причине, по которой разделены
/// сами порты (ISP): сценариям самообслуживания эти методы не нужны.
/// </summary>
internal sealed class InMemoryUserAdministrationService : IUserAdministrationService
{
    public int UserCount { get; set; }

    public List<AdminUserRowDto> Users { get; } = [];

    /// <summary>Что сохранил последний вызов <see cref="SetRolesAsync"/>.</summary>
    public Dictionary<Guid, IReadOnlyCollection<string>> SavedRoles { get; } = [];

    public Dictionary<Guid, bool> Blocked { get; } = [];

    public Dictionary<string, int> RoleCounts { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Task<int> CountUsersAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(UserCount);

    public Task<PagedResult<AdminUserRowDto>> SearchAsync(
        AdminUserFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var rows = Users.AsEnumerable();

        if (filter.IsBlocked is { } blocked)
        {
            rows = rows.Where(user => user.IsBlocked == blocked);
        }

        if (!string.IsNullOrWhiteSpace(filter.Role))
        {
            rows = rows.Where(user => user.Roles.Contains(filter.Role, StringComparer.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            rows = rows.Where(user => user.Email.Contains(filter.Search, StringComparison.OrdinalIgnoreCase));
        }

        rows = filter.NewestFirst
            ? rows.OrderByDescending(user => user.CreatedAt)
            : rows.OrderBy(user => user.Email, StringComparer.Ordinal);

        var all = rows.ToList();
        var page = all.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToList();

        return Task.FromResult(new PagedResult<AdminUserRowDto>(page, filter.Page, filter.PageSize, all.Count));
    }

    public Task<Result> SetBlockedAsync(Guid userId, bool isBlocked, CancellationToken cancellationToken = default)
    {
        Blocked[userId] = isBlocked;

        return Task.FromResult(Result.Success());
    }

    public Task<Result> SetRolesAsync(
        Guid userId,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default)
    {
        SavedRoles[userId] = roles;

        return Task.FromResult(Result.Success());
    }

    public Task<int> CountInRoleAsync(string role, CancellationToken cancellationToken = default) =>
        Task.FromResult(RoleCounts.GetValueOrDefault(role));
}