using System.Text.Json.Serialization;
using Shopping.Infrastructure.Interfaces;
using IEvent = Shopping.Infrastructure.Interfaces.IEvent;

namespace Shopping.Infrastructure.Persistence.Orders.Events;

[JsonDerivedType(typeof(OrderCreatedEvent), $"{nameof(OrderCreatedEvent)}")]
[JsonDerivedType(typeof(OrderCompletedEvent), $"{nameof(OrderCompletedEvent)}")]
[JsonDerivedType(typeof(OrderCancelledEvent), $"{nameof(OrderCancelledEvent)}")]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "EventType")]
public abstract record OrderEvent : IEvent;

public record OrderCreatedEvent(
    string Id,
    DateTime CreatedOnUtc,
    string CustomerId,
    uint Version,
    string CorrelationId,
    string CausationId) : OrderEvent, IPersistenceIdentifier;

public record OrderCompletedEvent(
    string Id,
    DateTime CompletedOnUtc,
    string CustomerId,
    string OrderId,
    uint Version,
    string CorrelationId,
    string CausationId) : OrderEvent, IPersistenceIdentifier;

public record OrderCancelledEvent(
    string Id,
    DateTime CancelledOnUtc,
    string CustomerId,
    string OrderId,
    uint Version,
    string CorrelationId,
    string CausationId) : OrderEvent, IPersistenceIdentifier;