using MediatR;

using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Common.Rules;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Admin.Teams.Commands.UpdateTeamByAdmin;

public sealed class UpdateTeamByAdminCommandHandler(
    IRepository<Team> teamRepository,
    IReadRepository<Team> teams,
    IQueryExecutor queryExecutor,
    IFileStorage storage,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateTeamByAdminCommand, Result>
{
    public async Task<Result> Handle(UpdateTeamByAdminCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var team = await teamRepository.GetByIdAsync(request.Id, cancellationToken);

        if (team is null)
        {
            return Result.Failure(EsportsErrors.TeamNotFound);
        }

        var name = request.Name.Trim();
        var normalized = name.ToLowerInvariant();

        if (await queryExecutor.AnyAsync(
                teams.Query().Where(other => other.Id != request.Id && other.Name.ToLower() == normalized),
                cancellationToken))
        {
            return Result.Failure(EsportsErrors.TeamNameTaken);
        }

        if (!await StoredImages.IsAcceptableAsync(request.Logo, cancellationToken))
        {
            return Result.Failure(FileErrors.InvalidImage);
        }

        var updated = DomainRules.Apply(() =>
        {
            team.UpdateProfile(
                name,
                request.Tag,
                CountryCode.From(request.CountryCode),
                request.Region,
                request.FoundedAt,
                request.Description);

            team.UpdateRating(request.Rating);
        });

        if (updated.IsFailure)
        {
            return updated;
        }

        await StoredImages.ReplaceAsync(
            storage,
            request.Logo,
            request.RemoveLogo,
            team.LogoPath,
            team.ChangeLogo,
            () => unitOfWork.SaveChangesAsync(cancellationToken),
            cancellationToken);

        return Result.Success();
    }
}