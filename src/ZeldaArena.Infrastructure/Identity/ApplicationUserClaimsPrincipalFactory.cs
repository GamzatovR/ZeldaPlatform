using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using ZeldaArena.Domain.Constants;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>Добавляет в cookie claim'ы, по которым Web строит политики, не заглядывая в UserManager.</summary>
public sealed class ApplicationUserClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user).ConfigureAwait(false);

        identity.AddClaim(new Claim(
            AppClaimTypes.EmailConfirmed,
            user.EmailConfirmed ? AppClaimTypes.True : "false"));

        identity.AddClaim(new Claim(
            AppClaimTypes.TwoFactorEnabled,
            user.TwoFactorEnabled ? AppClaimTypes.True : "false"));

        if (!string.IsNullOrWhiteSpace(user.DisplayName))
        {
            identity.AddClaim(new Claim(AppClaimTypes.DisplayName, user.DisplayName));
        }

        return identity;
    }
}