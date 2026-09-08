using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using ZeldaArena.Domain.Constants;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>
/// Добавляет в cookie claim'ы, по которым Web строит политики §8.1, не заглядывая
/// в UserManager (правило 3 docs/SPEC.md §5.2).
///
/// Claim — снимок на момент выдачи cookie, а не живое значение. Это осознанная плата:
/// стамп безопасности сверяется раз в пять минут (§8.2), а сценарии, меняющие эти
/// свойства, сами вызывают RefreshSignIn.
/// </summary>
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