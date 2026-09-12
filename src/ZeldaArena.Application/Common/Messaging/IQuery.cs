using MediatR;

namespace ZeldaArena.Application.Common.Messaging;

public interface IQuery<TResponse> : IRequest<TResponse>;