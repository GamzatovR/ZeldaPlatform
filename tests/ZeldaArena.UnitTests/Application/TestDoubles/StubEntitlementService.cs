using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

internal sealed class StubEntitlementService : IEntitlementService
{
    private readonly Dictionary<Guid, Dictionary<string, string?>> _grants = [];

    public StubEntitlementService Grant(Guid userId, string featureCode, string? value = null)
    {
        if (!_grants.TryGetValue(userId, out var features))
        {
            features = new Dictionary<string, string?>(StringComparer.Ordinal);
            _grants[userId] = features;
        }

        features[featureCode] = value;

        return this;
    }

    public void Revoke(Guid userId, string featureCode)
    {
        if (_grants.TryGetValue(userId, out var features))
        {
            features.Remove(featureCode);
        }
    }

    public Task<bool> HasFeatureAsync(Guid userId, string featureCode, CancellationToken cancellationToken = default) =>
        Task.FromResult(_grants.TryGetValue(userId, out var features) && features.ContainsKey(featureCode));

    public Task<EntitlementSet> GetEntitlementsAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_grants.TryGetValue(userId, out var features)
            ? new EntitlementSet(features)
            : EntitlementSet.Empty);

    public Task<string?> GetFeatureValueAsync(Guid userId, string featureCode, CancellationToken cancellationToken = default) =>
        Task.FromResult(_grants.TryGetValue(userId, out var features) && features.TryGetValue(featureCode, out var value)
            ? value
            : null);
}