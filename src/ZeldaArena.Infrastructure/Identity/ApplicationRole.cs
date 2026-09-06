using Microsoft.AspNetCore.Identity;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>
/// Роль: Admin, Moderator, User и техническая Premium. Premium выдаётся автоматически
/// и используется только для отображения — доступ дают фичи (docs/SPEC.md §7.4).
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole()
    {
    }

    public ApplicationRole(string roleName)
        : base(roleName)
    {
    }

    public string? Description { get; set; }
}