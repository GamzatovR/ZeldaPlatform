using MediatR;

using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Messaging;

/// <summary>Команда, возвращающая значение при успехе.</summary>
public interface ICommand<TResponse> : ICommandBase, IRequest<Result<TResponse>>;