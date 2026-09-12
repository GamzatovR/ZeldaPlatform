namespace ZeldaArena.Application.Common.Models.Billing;

public sealed record StartPaymentResult(Guid PaymentId, string MaskedEmail);