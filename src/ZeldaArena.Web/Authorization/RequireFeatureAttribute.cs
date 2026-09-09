using Microsoft.AspNetCore.Authorization;

namespace ZeldaArena.Web.Authorization;

/// <summary>
/// Декларативная проверка платной функции (docs/SPEC.md §7.3, уровень 2):
/// <c>[RequireFeature(FeatureCodes.TeamCreate)]</c>.
///
/// Код функции берётся из <c>FeatureCodes</c>, а не пишется литералом (CLAUDE.md).
/// Проверка роли Premium вместо этого атрибута запрещена: роль существует только
/// ради бейджа в разметке (§7.4, §20 пункт 2).
/// </summary>
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