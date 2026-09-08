using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Account.Commands.ResendEmailConfirmation;

/// <summary>
/// Просьба прислать письмо с подтверждением ещё раз: первое могло не дойти
/// или устареть.
/// </summary>
public sealed record ResendEmailConfirmationCommand(string Email) : ICommand;