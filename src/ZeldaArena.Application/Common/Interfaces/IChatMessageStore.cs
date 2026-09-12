using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.Application.Common.Interfaces;

public interface IChatMessageStore
{
    Task<ChatMessageRecord> AddAsync(
        ChatMessageRecord message,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChatMessageRecord>> GetHistoryAsync(
        Guid matchId,
        int limit,
        CancellationToken cancellationToken = default);

    /// <summary>Удаление сообщения модератором; <c>false</c>, если сообщения уже нет.</summary>
    Task<bool> DeleteAsync(string messageId, CancellationToken cancellationToken = default);
}