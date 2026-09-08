using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Account.Queries.GetTwoFactorStatus;

public sealed record GetTwoFactorStatusQuery : IQuery<TwoFactorStatusDto?>;