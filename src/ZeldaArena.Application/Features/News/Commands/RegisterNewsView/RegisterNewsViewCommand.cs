using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.News.Commands.RegisterNewsView;

public sealed record RegisterNewsViewCommand(Guid ArticleId) : ICommand;