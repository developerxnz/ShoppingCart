using System.Text.Json.Serialization;
using IEvent = Shopping.Infrastructure.Interfaces.IEvent;

namespace Shopping.Infrastructure.Persistence.Products;

[JsonDerivedType(typeof(ProductUpdatedEvent), $"{nameof(ProductUpdatedEvent)}")]
[JsonDerivedType(typeof(ProductCreatedEvent), $"{nameof(ProductCreatedEvent)}")]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "EventType")]
public abstract record ProductEvent : IEvent;

public record ProductUpdatedEvent(
    string ProductId,
    string Sku,
    string Description,
    decimal Amount,
    DateTime CreatedOnUtc,
    uint Version,
    string CorrelationId,
    string CausationId)
    : ProductEvent;

public record ProductCreatedEvent(
    string ProductId,
    string Sku,
    string Description,
    decimal Amount,
    DateTime CreatedOnUtc,
    uint Version,
    string CorrelationId,
    string CausationId)
    : ProductEvent;