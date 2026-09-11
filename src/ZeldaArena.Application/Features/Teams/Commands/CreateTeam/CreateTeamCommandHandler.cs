using MediatR;

using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Application.Common.Security;
using ZeldaArena.Application.Common.Slugs;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Teams.Commands.CreateTeam;

/// <summary>
/// Порядок проверок — от дешёвых к дорогим и от прав к данным: кто, есть ли у него
/// функция, не исчерпан ли лимит, свободно ли название, годен ли файл. Файл пишется
/// на диск последним, когда все отказы уже позади.
///
/// Функция проверяется здесь, а не только атрибутом на действии: тот же сценарий
/// вызовут Areas/Api в Фазе 8 и мобильный клиент (EP-2), и платная функция не должна
/// открываться со второго входа (docs/SPEC.md §7.3).
///
/// Лимит команд — параметр фичи <c>team.create</c> (<c>PlanFeature.Value</c>, EP-5, §15):
/// меняется в админке без правки кода. Нет значения — одна команда.
/// </summary>
public sealed class CreateTeamCommandHandler(
    IRepository<Team> teamRepository,
    IReadRepository<Team> teams,
    IQueryExecutor queryExecutor,
    IEntitlementService entitlements,
    ICurrentUserService currentUser,
    IFileStorage storage,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateTeamCommand, Result<string>>
{
    public const int DefaultTeamLimit = 1;

    public async Task<Result<string>> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (currentUser.UserId is not { } ownerId)
        {
            return Result.Failure<string>(AccountErrors.UserNotFound);
        }

        if (!await entitlements.CurrentUserHasAsync(currentUser, FeatureCodes.TeamCreate, cancellationToken)
                .ConfigureAwait(false))
        {
            return Result.Failure<string>(EsportsErrors.FeatureRequired);
        }

        var limit = await TeamLimitAsync(ownerId, cancellationToken).ConfigureAwait(false);
        var owned = await queryExecutor
            .CountAsync(teams.Query().Where(team => team.OwnerUserId == ownerId), cancellationToken)
            .ConfigureAwait(false);

        if (owned >= limit)
        {
            return Result.Failure<string>(EsportsErrors.TeamLimitReached);
        }

        var name = request.Name.Trim();
        var normalizedName = name.ToLowerInvariant();

        if (await queryExecutor
                .AnyAsync(teams.Query().Where(team => team.Name.ToLower() == normalizedName), cancellationToken)
                .ConfigureAwait(false))
        {
            return Result.Failure<string>(EsportsErrors.TeamNameTaken);
        }

        if (request.Logo is { } logo
            && await ImageFileInspector.DetectAsync(logo.Content, cancellationToken).ConfigureAwait(false) is null)
        {
            return Result.Failure<string>(FileErrors.InvalidImage);
        }

        var slug = await SlugGenerator.UniqueAsync(
            name,
            request.Tag,
            (candidate, token) => queryExecutor.AnyAsync(teams.Query().Where(team => team.Slug == candidate), token),
            cancellationToken).ConfigureAwait(false);

        var team = Team.CreateByUser(
            ownerId,
            slug,
            name,
            request.Tag,
            CountryCode.From(request.CountryCode),
            request.Region,
            request.FoundedAt,
            request.Description);

        var storedLogo = request.Logo is { } upload
            ? await storage.SaveAsync(upload.Content, upload.FileName, upload.ContentType, cancellationToken)
                .ConfigureAwait(false)
            : null;

        try
        {
            team.ChangeLogo(storedLogo?.StoredPath);

            await teamRepository.AddAsync(team, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            // Команда не сохранилась — файл без владельца на диске не нужен никому.
            if (storedLogo is not null)
            {
                await storage.DeleteAsync(storedLogo.StoredPath, CancellationToken.None).ConfigureAwait(false);
            }

            throw;
        }

        return slug.Value;
    }

    private async Task<int> TeamLimitAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        var value = await entitlements
            .GetFeatureValueAsync(ownerId, FeatureCodes.TeamCreate, cancellationToken)
            .ConfigureAwait(false);

        return int.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, out var limit) && limit > 0
            ? limit
            : DefaultTeamLimit;
    }
}