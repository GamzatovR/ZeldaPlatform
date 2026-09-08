using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Constants;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>
/// Создаёт роли и учётные записи, без которых портал не работает (docs/SPEC.md §6).
///
/// Идемпотентен, как и остальной сид: всё ищется по естественному ключу — имени роли
/// и адресу почты, — добавляется только недостающее. Повторный запуск ничего
/// не меняет и, что важнее, не сбрасывает пароль администратору, который его уже сменил.
/// </summary>
public sealed class IdentitySeeder(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IOptions<SeedAccountsOptions> options,
    IDateTimeProvider dateTimeProvider,
    ILogger<IdentitySeeder> logger)
{
    private readonly SeedAccountsOptions _accounts = options.Value;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync(cancellationToken).ConfigureAwait(false);

        await SeedAccountAsync(_accounts.Admin, RoleNames.Admin, cancellationToken)
            .ConfigureAwait(false);

        await SeedAccountAsync(_accounts.Moderator, RoleNames.Moderator, cancellationToken)
            .ConfigureAwait(false);

        foreach (var demo in _accounts.Demo)
        {
            await SeedAccountAsync(demo, RoleNames.User, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        foreach (var role in RoleNames.All)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await roleManager.RoleExistsAsync(role).ConfigureAwait(false))
            {
                continue;
            }

            var created = await roleManager
                .CreateAsync(new ApplicationRole(role) { Description = DescriptionOf(role) })
                .ConfigureAwait(false);

            if (created.Succeeded)
            {
                logger.LogInformation("Сид ролей: создана роль {Role}.", role);
            }
            else
            {
                logger.LogError(
                    "Сид ролей: не удалось создать роль {Role}. {Errors}",
                    role,
                    string.Join("; ", created.Errors.Select(error => error.Description)));
            }
        }
    }

    private async Task SeedAccountAsync(
        SeedAccountOptions account,
        string role,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!account.IsConfigured)
        {
            // Не ошибка: на боевом сервере учётные записи заводятся руками,
            // а адрес и пароль администратора приходят переменными окружения.
            logger.LogInformation("Сид учётных записей: роль {Role} пропущена, нет настроек.", role);

            return;
        }

        var existing = await userManager.FindByEmailAsync(account.Email).ConfigureAwait(false);

        if (existing is not null)
        {
            await EnsureRoleAsync(existing, role).ConfigureAwait(false);

            return;
        }

        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = account.Email,
            Email = account.Email,
            DisplayName = account.DisplayName,
            PreferredCulture = SupportedCultures.Default,
            CreatedAt = dateTimeProvider.UtcNow,

            // Демонстрационные учётные записи сразу пригодны для входа: заставлять
            // проверяющего искать письмо подтверждения для сидовых аккаунтов незачем.
            EmailConfirmed = true,
        };

        var created = await userManager.CreateAsync(user, account.Password).ConfigureAwait(false);

        if (!created.Succeeded)
        {
            logger.LogError(
                "Сид учётных записей: не удалось создать {Email}. {Errors}",
                account.Email,
                string.Join("; ", created.Errors.Select(error => error.Description)));

            return;
        }

        await EnsureRoleAsync(user, role).ConfigureAwait(false);

        logger.LogInformation(
            "Сид учётных записей: создан пользователь {Email} с ролью {Role}.",
            account.Email,
            role);
    }

    private async Task EnsureRoleAsync(ApplicationUser user, string role)
    {
        if (!await userManager.IsInRoleAsync(user, role).ConfigureAwait(false))
        {
            await userManager.AddToRoleAsync(user, role).ConfigureAwait(false);
        }

        // Роль User есть у всех: политики §8.1 опираются на неё как на признак
        // зарегистрированного пользователя.
        if (role != RoleNames.User
            && !await userManager.IsInRoleAsync(user, RoleNames.User).ConfigureAwait(false))
        {
            await userManager.AddToRoleAsync(user, RoleNames.User).ConfigureAwait(false);
        }
    }

    private static string DescriptionOf(string role) => role switch
    {
        RoleNames.Admin => "Полный доступ: пользователи, роли, тарифы, фичи, заказы, аудит.",
        RoleNames.Moderator => "Турниры, матчи, команды, игроки, новости, модерация комментариев.",
        RoleNames.User => "Публичная часть и личный кабинет.",
        RoleNames.Premium => "Техническая роль-бейдж. Права даёт подписка, а не она.",
        _ => string.Empty,
    };
}