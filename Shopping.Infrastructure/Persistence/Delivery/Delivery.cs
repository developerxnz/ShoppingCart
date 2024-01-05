using System.Text.Json.Serialization;
using Shopping.Infrastructure.Interfaces;

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