namespace ZeldaArena.Application.Features.Billing;

public static class BillingAudit
{
    public const string Payment = nameof(Domain.Billing.Payment);

    public const string Subscription = nameof(Domain.Billing.Subscription);

    public const string Plan = nameof(Domain.Billing.Plan);

    public const string Feature = nameof(Domain.Billing.Feature);
}