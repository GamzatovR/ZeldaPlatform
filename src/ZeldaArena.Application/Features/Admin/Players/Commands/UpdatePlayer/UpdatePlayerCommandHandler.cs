using MediatR;

using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Common.Rules;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Admin.Players.Commands.UpdatePlayer;

public sealed class UpdatePlayerCommandHandler(
    IRepository<Player> players,
    IFileStorage storage,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdatePlayerCommand, Result>
{
    public async Task<Result> Handle(UpdatePlayerCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var player = await players.GetByIdAsync(request.Id, cancellationToken);

        if (player is null)
        {
            return Result.Failure(EsportsErrors.PlayerNotFound);
        }

        if (!await StoredImages.IsAcceptableAsync(request.Avatar, cancellationToken))
        {
            return Result.Failure(FileErrors.InvalidImage);
        }

        var updated = DomainRules.Apply(() => player.UpdateProfile(
            request.Nickname,
            CountryCode.From(request.CountryCode),
            request.Role,
            request.FirstName,
            request.LastName,
            request.BirthDate,
            request.Bio));

        if (updated.IsFailure)
        {
            return updated;
        }

        await StoredImages.ReplaceAsync(
            storage,
            request.Avatar,
            request.RemoveAvatar,
            player.AvatarPath,
            player.ChangeAvatar,
            () => unitOfWork.SaveChangesAsync(cancellationToken),
            cancellationToken);

        return Result.Success();
    }
}