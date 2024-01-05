using System.Text.Json.Serialization;
using Shopping.Infrastructure.Interfaces;

namespace Shopping.Infrastructure.Persistence.Cart;

public record Cart : IPersistenceIdentifier
{
    public string PartitionKey => CustomerId;

    public string Id { get; init; }

    public string CustomerId { get; init; }

    public DateTime CreatedOnUtc { get; init; }

    public Domain.Core.Persistence.Metadata Metadata { get; init; }

    public IEnumerable<CartItem> Items { get; init; }

    [JsonPropertyName("_etag")] public string Etag { get; init; }
}