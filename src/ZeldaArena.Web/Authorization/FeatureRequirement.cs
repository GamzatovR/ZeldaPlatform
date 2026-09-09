using Microsoft.AspNetCore.Authorization;

namespace ZeldaArena.Web.Authorization;

/// <summary>
/// Требование доступа к платной функции (docs/SPEC.md §7.3).
///
/// Отдельный тип требования нужен не только обработчику: по нему
/// <see cref="FeatureAccessDeniedHandler"/> отличает «нет подписки» от «не хватает
/// прав» и отвечает предложением оформить подписку вместо голого 403.
/// </summary>
public sealed class FeatureRequirement(string featureCode) : IAuthorizationRequirement
{
    public string FeatureCode { get; } = featureCode;
}