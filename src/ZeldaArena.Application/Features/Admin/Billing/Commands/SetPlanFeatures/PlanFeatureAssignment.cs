namespace ZeldaArena.Application.Features.Admin.Billing.Commands.SetPlanFeatures;

public sealed record PlanFeatureAssignment(Guid FeatureId, string? Value);