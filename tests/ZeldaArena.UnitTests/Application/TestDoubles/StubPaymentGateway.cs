using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

internal sealed class StubPaymentGateway : IPaymentGateway
{
    public string Key => "stub";

    public bool Declines { get; set; }

    public CardPaymentRequest? LastRequest { get; private set; }

    public Task<Result<CardAuthorization>> AuthorizeAsync(
        CardPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        LastRequest = request;

        return Task.FromResult(Declines
            ? Result.Failure<CardAuthorization>(BillingErrors.CardDeclined)
            : Result.Success(new CardAuthorization("Visa", "4242")));
    }
}