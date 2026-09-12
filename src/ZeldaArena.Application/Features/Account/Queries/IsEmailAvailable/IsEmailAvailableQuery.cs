using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Account.Queries.IsEmailAvailable;

public sealed record IsEmailAvailableQuery(string Email) : IQuery<bool>;