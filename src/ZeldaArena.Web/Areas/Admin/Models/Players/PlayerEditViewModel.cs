using ZeldaArena.Application.Features.Admin.Players.Queries.GetPlayerForEdit;

namespace ZeldaArena.Web.Areas.Admin.Models.Players;

public sealed class PlayerEditViewModel
{
    public required PlayerEditDto Player { get; init; }

    public required PlayerFormViewModel Form { get; init; }
}