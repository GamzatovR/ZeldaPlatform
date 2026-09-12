using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ZeldaArena.Web.Authorization;

/// <summary>Собирает политику Feature:{code} на лету.</summary>
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