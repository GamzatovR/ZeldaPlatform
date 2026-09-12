using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Matches.Commands.UpdateMatchScore;

/// <summary>
/// Изменение счёта матча из пульта модератора (docs/SPEC.md §11). Главный сценарий
/// демонстрации SignalR: счёт меняется в одном окне и мгновенно появляется в другом.
///
/// Команда помечена как аудируемая: правка счёта — именно то действие, о котором
/// нужно знать, кто и когда его сделал (§13).
/// </summary>
/// <param name="MatchId">Матч.</param>
/// <param name="ScoreA">Новый счёт первой команды.</param>
/// <param name="ScoreB">Новый счёт второй команды.</param>
/// <param name="ExpectedScoreA">
/// Счёт, который модератор видел в пульте, — защита от устаревшей вкладки (§15,
/// «два модератора правят счёт одного матча»). Не совпал с текущим — правка
/// отклоняется как конфликт. <see langword="null"/> означает «счёт не сверять»:
/// так вызывают сценарий тесты и клиенты без пульта.
/// </param>
/// <param name="ExpectedScoreB">То же для второй команды.</param>
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