using MediatR;

using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Events;

public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification
    where TDomainEvent : DomainEvent;