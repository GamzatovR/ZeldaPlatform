using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Admin.Teams.Queries.GetTeamForEdit;

/// <summary>Карточка команды в админке: профиль, состав, свободные игроки.</summary>
public sealed record GetTeamForEditQuery(Guid Id) : IQuery<TeamEditDto?>;