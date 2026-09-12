using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Matches.Commands.UpdateMatchScore;

public sealed record UpdateMatchScoreCommand(
    Guid MatchId,
    int ScoreA,
    int ScoreB,
    int? ExpectedScoreA = null,
    int? ExpectedScoreB = null)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Domain.Esports.Match);

    public string? AuditEntityId => MatchId.ToString();
}