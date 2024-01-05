using System.Text.Json.Serialization;
using Shopping.Infrastructure.Interfaces;
using IEvent = Shopping.Infrastructure.Interfaces.IEvent;

namespace Shopping.Infrastructure.Persistence.Cart;


[JsonDerivedType(typeof(CartItemAddedEvent), $"{nameof(CartItemAddedEvent)}")]
[JsonDerivedType(typeof(CartItemRemovedEvent), $"{nameof(CartItemRemovedEvent)}")]
[JsonDerivedType(typeof(CartItemUpdatedEvent), $"{nameof(CartItemUpdatedEvent)}")]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "EventType")]
public abstract record CartEvent : IEvent;



public record CartItemAddedEvent(
    string Id,
    string CustomerId,
    DateTime AddedOnUtc,
    string Sku,
    uint Quantity,
    uint Version,
    string CorrelationId,
    string CausationId)
    : CartEvent, IPersistenceIdentifier { }

public record CartItemRemovedEvent(
    string Id,
    string CustomerId,
    DateTime RemovedOnUtc,
    string Sku,
    uint Version,
    string CorrelationId,
    string CausationId)
    : CartEvent, IPersistenceIdentifier { }

public record CartItemUpdatedEvent(
    string Id,
    string CustomerId,
    DateTime UpdatedOnUtc,
    string Sku,
    uint Quantity,
    uint Version,
    string CorrelationId,
    string CausationId)
    : CartEvent, IPersistenceIdentifier { }