using MediatR;

using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Messaging;

/// <summary>
/// Команда без возвращаемого значения. Всегда отвечает <see cref="Result"/>:
/// ожидаемая неудача сценария (не нашли, не владелец, нельзя по правилу) — это
/// результат, а не исключение. Исключения остаются за нарушениями инвариантов домена
/// и за поломками инфраструктуры.
/// </summary>
public interface ICommand : ICommandBase, IRequest<Result>;