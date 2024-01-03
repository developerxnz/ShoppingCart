using System.Text.Json.Serialization;
using Shopping.Infrastructure.Interfaces;
using IEvent = Shopping.Infrastructure.Interfaces.IEvent;

namespace Shopping.Infrastructure.Persistence.Cart;

public record CartAggregate : IPersistenceIdentifier
{
    public string PartitionKey => CustomerId;

    public string Id { get; init; }

    public string CustomerId { get; init; }

    public DateTime CreatedOnUtc { get; init; }

    public Domain.Core.Persistence.Metadata Metadata { get; init; }

    public IEnumerable<CartItem> Items { get; init; }

    [JsonPropertyName("_etag")] public string Etag { get; init; }
}

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
    : CartEvent, IPersistenceIdentifier
{
}

public record CartItemRemovedEvent(
    string Id,
    string CustomerId,
    DateTime RemovedOnUtc,
    string Sku,
    uint Version,
    string CorrelationId,
    string CausationId)
    : CartEvent, IPersistenceIdentifier
{
}

public record CartItemUpdatedEvent(
    string Id,
    string CustomerId,
    DateTime UpdatedOnUtc,
    string Sku,
    uint Quantity,
    uint Version,
    string CorrelationId,
    string CausationId)
    : CartEvent, IPersistenceIdentifier
{
}