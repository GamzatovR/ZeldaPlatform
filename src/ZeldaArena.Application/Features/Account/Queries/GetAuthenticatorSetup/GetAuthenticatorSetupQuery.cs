using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models.Identity;

namespace ZeldaArena.Application.Features.Account.Queries.GetAuthenticatorSetup;

/// <summary>
/// Ключ и ссылка otpauth:// для подключения аутентификатора. Всегда о текущем
/// пользователе: чужой ключ — это чужой второй фактор.
/// </summary>
public sealed record GetAuthenticatorSetupQuery : IQuery<TwoFactorSetup?>;