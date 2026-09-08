using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ZeldaArena.Domain.Constants;
using ZeldaArena.Infrastructure.Persistence.Ef;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>
/// Подключение ASP.NET Core Identity и настройка политики безопасности аккаунта
/// (docs/SPEC.md §8.2). Схема таблиц AspNet* уже лежит в первой миграции
/// (docs/adr/ADR-0003), поэтому здесь добавляются только сервисы.
///
/// Маршруты страниц входа и отказа в доступе здесь не задаются намеренно: они
/// относятся к слою представления и настраиваются в Program.cs через
/// ConfigureApplicationCookie. Infrastructure отвечает за то, каким должен быть
/// пароль и как долго живёт cookie, а не за то, где лежит страница входа.
/// </summary>
internal static class IdentityRegistration
{
    /// <summary>
    /// Когда HTTPS не поднят, cookie с флагом Secure браузер не сохранит и вход
    /// молча перестанет работать. Профиль http из launchSettings — как раз этот случай,
    /// поэтому в разработке флаг снимается настройкой, а не правкой кода.
    /// </summary>
    private const string RequireSecureCookieKey = "Identity:RequireSecureCookie";

    public static IServiceCollection AddPlatformIdentity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var requireSecureCookie = configuration.GetValue(RequireSecureCookieKey, defaultValue: true);

        services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // Требования читаются из PasswordPolicy: те же константы проверяет
                // FluentValidation в Application, поэтому форма и Identity не могут
                // разойтись в том, какой пароль считать годным.
                options.Password.RequiredLength = PasswordPolicy.MinimumLength;
                options.Password.RequireDigit = PasswordPolicy.RequireDigit;
                options.Password.RequireLowercase = PasswordPolicy.RequireLowercase;
                options.Password.RequireUppercase = PasswordPolicy.RequireUppercase;
                options.Password.RequireNonAlphanumeric = PasswordPolicy.RequireNonAlphanumeric;

                options.Lockout.MaxFailedAccessAttempts = IdentityPolicy.MaxFailedAccessAttempts;
                options.Lockout.DefaultLockoutTimeSpan = IdentityPolicy.LockoutDuration;
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;

                // Без подтверждённой почты вход запрещён: на неё уходят ссылки сброса
                // пароля и коды подтверждения оплаты (§7.6).
                options.SignIn.RequireConfirmedEmail = true;

                options.Tokens.PasswordResetTokenProvider =
                    IdentityPolicy.PasswordResetTokenProviderName;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<PasswordResetTokenProvider<ApplicationUser>>(
                IdentityPolicy.PasswordResetTokenProviderName);

        services.Configure<PasswordResetTokenProviderOptions>(options =>
            options.TokenLifespan = IdentityPolicy.PasswordResetTokenLifetime);

        // Смена пароля или блокировка пользователя должны сказываться на уже выданных
        // cookie в пределах пяти минут, а не через две недели (§8.2).
        services.Configure<SecurityStampValidatorOptions>(options =>
            options.ValidationInterval = IdentityPolicy.SecurityStampValidationInterval);

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = requireSecureCookie
                ? CookieSecurePolicy.Always
                : CookieSecurePolicy.SameAsRequest;

            options.ExpireTimeSpan = IdentityPolicy.SignInCookieLifetime;
            options.SlidingExpiration = true;
        });

        services.Configure<CookieAuthenticationOptions>(
            IdentityConstants.TwoFactorRememberMeScheme,
            options => options.ExpireTimeSpan = IdentityPolicy.RememberDeviceDuration);

        return services;
    }
}