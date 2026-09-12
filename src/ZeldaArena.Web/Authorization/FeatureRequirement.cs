using Microsoft.AspNetCore.Authorization;

namespace ZeldaArena.Web.Authorization;

/// <summary>Требование доступа к платной функции.</summary>
public sealed class FeatureRequirement(string featureCode) : IAuthorizationRequirement
{
    public string FeatureCode { get; } = featureCode;
}