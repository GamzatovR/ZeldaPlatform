using MediatR;

using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Messaging;

public interface ICommand : ICommandBase, IRequest<Result>;