using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Account.Commands.ForgotPassword;

/// <summary>Запрос ссылки для сброса пароля (docs/SPEC.md §8.2).</summary>
public sealed record ForgotPasswordCommand(string Email) : ICommand;