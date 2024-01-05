using System.Text.Json.Serialization;
using Shopping.Infrastructure.Interfaces;

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