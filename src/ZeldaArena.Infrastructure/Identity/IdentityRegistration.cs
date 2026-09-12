using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ZeldaArena.Domain.Constants;
using ZeldaArena.Infrastructure.Persistence.Ef;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>Подключение ASP.NET Core Identity и настройка политики безопасности аккаунта.</summary>
internal static class IdentityRegistration
{
    private const string RequireSecureCookieKey = "Identity:RequireSecureCookie";

    public static IServiceCollection AddPlatformIdentity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var requireSecureCookie = configuration.GetValue(RequireSecureCookieKey, defaultValue: true);

        services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // Требования читаются из PasswordPolicy.
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
                // пароля и коды подтверждения оплаты.
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
        // cookie в пределах пяти минут, а не через две недели.
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