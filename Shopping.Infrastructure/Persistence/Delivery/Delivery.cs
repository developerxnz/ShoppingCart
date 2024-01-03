using System.Text.Json.Serialization;
using Shopping.Infrastructure.Interfaces;
using IEvent = Shopping.Infrastructure.Interfaces.IEvent;

namespace Shopping.Infrastructure.Persistence.Delivery;

public record Delivery : IPersistenceIdentifier
{
    public string PartitionKey => OrderId;

    public string Id { get; init; }

    public string OrderId { get; init; }

    public DateTime CreatedOnUtc { get; init; }

    public DateTime? DeliveredOnUtc { get; init; }

    public Domain.Core.Persistence.Metadata Metadata { get; init; }

    [JsonPropertyName("_etag")] public string Etag { get; init; }
}

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