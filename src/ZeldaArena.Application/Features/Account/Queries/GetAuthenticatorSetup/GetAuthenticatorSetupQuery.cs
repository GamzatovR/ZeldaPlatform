using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models.Identity;

namespace ZeldaArena.Application.Features.Account.Queries.GetAuthenticatorSetup;

public sealed record GetAuthenticatorSetupQuery : IQuery<TwoFactorSetup?>;