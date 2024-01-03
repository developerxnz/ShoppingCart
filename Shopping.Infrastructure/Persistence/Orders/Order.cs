using System.Text.Json.Serialization;
using Shopping.Infrastructure.Interfaces;
using IEvent = Shopping.Infrastructure.Interfaces.IEvent;

namespace Shopping.Infrastructure.Persistence.Orders;

public record Order : IPersistenceIdentifier
{
    public string PartitionKey => CustomerId;

    public string Id { get; init; }

    public string CustomerId { get; init; }

    public DateTime? CancelledOnUtc { get; init; }
    
    public DateTime? CompletedOnUtc { get; init; }
    
    public DateTime CreatedOnUtc { get; init; }

    public Domain.Core.Persistence.Metadata Metadata { get; init; }

    [JsonPropertyName("_etag")] public string Etag { get; init; }
}

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