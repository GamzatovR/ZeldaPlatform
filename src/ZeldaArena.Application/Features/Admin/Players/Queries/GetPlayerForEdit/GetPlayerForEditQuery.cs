using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Admin.Players.Queries.GetPlayerForEdit;

public sealed record GetPlayerForEditQuery(Guid Id) : IQuery<PlayerEditDto?>;