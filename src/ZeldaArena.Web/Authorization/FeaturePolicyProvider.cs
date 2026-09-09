using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ZeldaArena.Web.Authorization;

/// <summary>
/// Собирает политику <c>Feature:{code}</c> на лету (docs/SPEC.md §7.3, уровень 2).
///
/// Это и есть механизм EP-3: новая платная функция заводится строкой в таблице
/// Features через админку, а её политика возникает сама, как только на действии
/// появляется атрибут с кодом. Списка политик, который надо было бы пополнять
/// при каждой новой функции, не существует.
///
/// Всё, что не начинается с префикса, отдаётся провайдеру по умолчанию — статические
/// политики §8.1 из AuthorizationRegistration продолжают работать без изменений.
/// </summary>
public sealed class FeaturePolicyProvider(IOptions<AuthorizationOptions> options)
    : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback = new(options);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        ArgumentNullException.ThrowIfNull(policyName);

        var featureCode = FeaturePolicy.FeatureCodeOf(policyName);

        if (featureCode is null)
        {
            return _fallback.GetPolicyAsync(policyName);
        }

        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new FeatureRequirement(featureCode))
            .Build();

        return Task.FromResult<AuthorizationPolicy?>(policy);
    }
}