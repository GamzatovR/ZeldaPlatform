using ZeldaArena.Application.Common.Files;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Players;

/// <summary>Поля профиля игрока — общие для создания и правки.</summary>
public interface IPlayerFields
{
    string Nickname { get; }

    string CountryCode { get; }

    PlayerRole Role { get; }

    string? FirstName { get; }

    string? LastName { get; }

    DateOnly? BirthDate { get; }

    string? Bio { get; }

    FileUpload? Avatar { get; }
}