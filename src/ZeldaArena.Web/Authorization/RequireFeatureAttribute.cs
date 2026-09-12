using Microsoft.AspNetCore.Authorization;

namespace ZeldaArena.Web.Authorization;

/// <summary>Декларативная проверка платной функции.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequireFeatureAttribute : AuthorizeAttribute
{
    public RequireFeatureAttribute(string featureCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureCode);

        FeatureCode = featureCode;
        Policy = FeaturePolicy.NameFor(featureCode);
    }

    public string FeatureCode { get; }
}