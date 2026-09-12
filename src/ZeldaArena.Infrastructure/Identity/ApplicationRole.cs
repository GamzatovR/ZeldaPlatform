using Microsoft.AspNetCore.Identity;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>Роль.</summary>
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