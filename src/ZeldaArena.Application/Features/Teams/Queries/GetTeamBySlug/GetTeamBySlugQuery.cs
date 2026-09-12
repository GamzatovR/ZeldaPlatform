using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Teams.Queries.GetTeamBySlug;

public sealed record GetTeamBySlugQuery(string Slug) : IQuery<TeamDetailsDto?>;