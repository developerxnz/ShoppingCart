using System.Text.Json.Serialization;
using IEvent = Shopping.Infrastructure.Interfaces.IEvent;

namespace Shopping.Infrastructure.Persistence.Delivery;

[JsonDerivedType(typeof(DeliveryCancelledEvent), $"{nameof(DeliveryCancelledEvent)}")]
[JsonDerivedType(typeof(DeliveryCompletedEvent), $"{nameof(DeliveryCompletedEvent)}")]
[JsonDerivedType(typeof(DeliveryCreatedEvent), $"{nameof(DeliveryCreatedEvent)}")]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "EventType")]
public abstract record DeliveryEvent : IEvent;

public record DeliveryCreatedEvent(
    DateTime CreatedOnUtc,
    string CustomerId,
    string OrderId,
    uint Version,
    string CorrelationId,
    string CausationId):DeliveryEvent;

public record DeliveryCompletedEvent(
    DateTime CompletedOnUtc,
    string CustomerId,
    string DeliveryId,
    string OrderId,
    uint Version,
    string CorrelationId,
    string CausationId):DeliveryEvent;

public record DeliveryCancelledEvent(
    DateTime CancelledOnUtc,
    string CustomerId,
    string DeliveryId,
    string OrderId,
    uint Version,
    string CorrelationId,
    string CausationId):DeliveryEvent;