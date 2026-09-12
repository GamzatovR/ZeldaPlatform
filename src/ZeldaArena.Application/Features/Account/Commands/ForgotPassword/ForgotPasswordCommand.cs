using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Account.Commands.ForgotPassword;

/// <summary>Запрос ссылки для сброса пароля.</summary>
public sealed record ForgotPasswordCommand(string Email) : ICommand;