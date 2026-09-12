using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Account.Commands.ResendEmailConfirmation;

public sealed record ResendEmailConfirmationCommand(string Email) : ICommand;