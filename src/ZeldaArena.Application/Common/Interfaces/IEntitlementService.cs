using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.Application.Common.Interfaces;

public interface IEntitlementService
{
    Task<bool> HasFeatureAsync(
        Guid userId,
        string featureCode,
        CancellationToken cancellationToken = default);

    Task<EntitlementSet> GetEntitlementsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<string?> GetFeatureValueAsync(
        Guid userId,
        string featureCode,
        CancellationToken cancellationToken = default);
}