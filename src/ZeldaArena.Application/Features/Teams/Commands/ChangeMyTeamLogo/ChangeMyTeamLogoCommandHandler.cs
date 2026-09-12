using MediatR;

using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Teams.Commands.ChangeMyTeamLogo;

public sealed class ChangeMyTeamLogoCommandHandler(
    IRepository<Team> teamRepository,
    IEntitlementService entitlements,
    ICurrentUserService currentUser,
    IFileStorage storage,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeMyTeamLogoCommand, Result>
{
    public async Task<Result> Handle(ChangeMyTeamLogoCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var access = await MyTeamAccess
            .LoadEditableAsync(teamRepository, entitlements, currentUser, request.TeamId, cancellationToken)
            .ConfigureAwait(false);

        if (access.IsFailure)
        {
            return Result.Failure(access.Error);
        }

        if (await ImageFileInspector.DetectAsync(request.Logo.Content, cancellationToken).ConfigureAwait(false) is null)
        {
            return Result.Failure(FileErrors.InvalidImage);
        }

        var team = access.Value;
        var previous = team.LogoPath;

        var stored = await storage
            .SaveAsync(request.Logo.Content, request.Logo.FileName, request.Logo.ContentType, cancellationToken)
            .ConfigureAwait(false);

        try
        {
            team.ChangeLogo(stored.StoredPath);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await storage.DeleteAsync(stored.StoredPath, CancellationToken.None).ConfigureAwait(false);
            throw;
        }

        if (previous is not null)
        {
            await storage.DeleteAsync(previous, cancellationToken).ConfigureAwait(false);
        }

        return Result.Success();
    }
}