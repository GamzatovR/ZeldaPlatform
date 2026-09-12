using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Interfaces;

public interface IPaymentGateway
{
    string Key { get; }

    Task<Result<CardAuthorization>> AuthorizeAsync(
        CardPaymentRequest request,
        CancellationToken cancellationToken = default);
}