using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Teams.Commands.CreateTeam;

/// <summary>
/// Создание своей команды подписчиком — главная платная функция (docs/SPEC.md §4, §9.3 п. 8).
/// Возвращает слаг созданной команды. Пишется в аудит: создание команды — обязательное
/// событие по §8.2.
/// </summary>
public sealed record CreateTeamCommand : ICommand<string>, IAuditableRequest
{
    public required string Name { get; init; }

    public required string Tag { get; init; }

    public required string CountryCode { get; init; }

    public Region Region { get; init; }

    public DateOnly? FoundedAt { get; init; }

    public string? Description { get; init; }

    public FileUpload? Logo { get; init; }

    public string AuditEntityType => nameof(Team);

    public string? AuditEntityId => null;
}