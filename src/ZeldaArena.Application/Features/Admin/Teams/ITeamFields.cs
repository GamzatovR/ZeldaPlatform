using ZeldaArena.Application.Common.Files;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Teams;

/// <summary>Поля профиля команды — общие для создания и правки в админке.</summary>
public interface ITeamFields
{
    string Name { get; }

    string Tag { get; }

    string CountryCode { get; }

    Region Region { get; }

    DateOnly? FoundedAt { get; }

    string? Description { get; }

    FileUpload? Logo { get; }
}