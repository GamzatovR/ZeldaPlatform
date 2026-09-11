using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Tournaments.Queries.GetTournamentBySlug;

/// <summary>
/// Один запрос на турнир вместе с участниками: состав участников — вложенная проекция,
/// и EF Core разворачивает её в JOIN, а не в запрос на каждую команду (docs/SPEC.md §16,
/// обязательная проверка на N+1 на странице турнира).
///
/// Слаг сравнивается как объект-значение, а не строкой: у столбца стоит конвертер,
/// и сравнение по <c>Slug.Value</c> EF Core в SQL не переведёт.
/// </summary>
public sealed class GetTournamentBySlugQueryHandler(
    IReadRepository<Tournament> tournaments,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetTournamentBySlugQuery, TournamentDetailsDto?>
{
    public Task<TournamentDetailsDto?> Handle(GetTournamentBySlugQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Ссылку, из которой слаг не складывается, ищем так же, как несуществующую:
        // это просто «такого турнира нет».
        if (!Slug.TryFrom(request.Slug, out var slug) || slug is null)
        {
            return Task.FromResult<TournamentDetailsDto?>(null);
        }

        var query = tournaments
            .Query()
            .Where(tournament => tournament.Slug == slug)
            .Select(tournament => new TournamentDetailsDto
            {
                Id = tournament.Id,
                Slug = tournament.Slug.Value,
                Name = tournament.Name,
                Description = tournament.Description,
                Tier = tournament.Tier,
                Region = tournament.Region,
                PrizePoolAmount = tournament.PrizePool.Amount,
                PrizePoolCurrency = tournament.PrizePool.Currency,
                StartsAt = tournament.StartsAt,
                EndsAt = tournament.EndsAt,
                Status = tournament.Status,
                RulesHtml = tournament.RulesHtml,
                LogoPath = tournament.LogoPath,
                BannerPath = tournament.BannerPath,
                IsFeatured = tournament.IsFeatured,

                // Сначала занявшие места, затем по посеву: у идущего турнира мест ещё нет.
                Participants = tournament.Participants
                    .OrderBy(participant => participant.Placement == null)
                    .ThenBy(participant => participant.Placement)
                    .ThenBy(participant => participant.Seed)
                    .Select(participant => new TournamentParticipantDto
                    {
                        TeamId = participant.TeamId,
                        TeamSlug = participant.Team == null ? string.Empty : participant.Team.Slug.Value,
                        TeamName = participant.Team == null ? string.Empty : participant.Team.Name,
                        TeamTag = participant.Team == null ? string.Empty : participant.Team.Tag,
                        TeamLogoPath = participant.Team == null ? null : participant.Team.LogoPath,
                        Rating = participant.Team == null ? 0 : participant.Team.Rating,
                        Seed = participant.Seed,
                        Placement = participant.Placement,
                    })
                    .ToList(),
            });

        return queryExecutor.FirstOrDefaultAsync(query, cancellationToken);
    }
}