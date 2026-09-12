using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Web.Areas.Admin.Models.Teams;

/// <summary>Игрок и его роль — маленькая форма в строке состава.</summary>
public sealed class RosterInputModel
{
    public Guid PlayerId { get; set; }

    public PlayerRole Role { get; set; }
}