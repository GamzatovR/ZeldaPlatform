using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Players.Queries.GetPlayerBySlug;

public sealed record GetPlayerBySlugQuery(string Slug) : IQuery<PlayerDetailsDto?>;