using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Queries.GetTournamentForEdit;

public sealed class GetTournamentForEditQueryHandler(
    IReadRepository<Tournament> tournaments,
    IReadRepository<Team> teams,
    IReadRepository<Match> matches,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetTournamentForEditQuery, TournamentEditDto?>
{
    public async Task<TournamentEditDto?> Handle(GetTournamentForEditQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var id = request.Id;

        var tournament = await queryExecutor.FirstOrDefaultAsync(
            tournaments.Query()
                .Where(item => item.Id == id)
                .Select(item => new TournamentEditDto
                {
                    Id = item.Id,
                    Slug = item.Slug.Value,
                    Name = item.Name,
                    Tier = item.Tier,
                    Region = item.Region,
                    Status = item.Status,
                    StartsAt = item.StartsAt,
                    EndsAt = item.EndsAt,
                    PrizePool = item.PrizePool.Amount,
                    Currency = item.PrizePool.Currency,
                    Description = item.Description,
                    RulesHtml = item.RulesHtml,
                    LogoPath = item.LogoPath,
                    IsFeatured = item.IsFeatured,
                    MatchCount = matches.Query().Count(match => match.TournamentId == id),
                }),
            cancellationToken);

        if (tournament is null)
        {
            return null;
        }

        var participants = await queryExecutor.ToListAsync(
            from participant in tournaments.Query().Where(item => item.Id == id).SelectMany(item => item.Participants)
            join team in teams.Query() on participant.TeamId equals team.Id
            orderby participant.Seed, team.Name
            select new TournamentParticipantDto(
                participant.TeamId,
                team.Name,
                team.Slug.Value,
                participant.Seed,
                participant.Placement,
                matches.Query().Count(match => match.TournamentId == id
                    && (match.TeamAId == participant.TeamId || match.TeamBId == participant.TeamId))),
            cancellationToken);

        var participantIds = participants.Select(participant => participant.TeamId).ToList();

        var available = await queryExecutor.ToListAsync(
            teams.Query()
                .Where(team => team.IsApproved && !participantIds.Contains(team.Id))
                .OrderBy(team => team.Name)
                .Select(team => new TeamOptionDto(team.Id, team.Name, team.Tag)),
            cancellationToken);

        return tournament with { Participants = participants, AvailableTeams = available };
    }
}