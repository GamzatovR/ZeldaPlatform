using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Common.Models;

public sealed record CardPaymentRequest(
    string CardNumber,
    int ExpiryMonth,
    int ExpiryYear,
    string Cvv,
    Money Amount);