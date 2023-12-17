using Microsoft.Azure.Cosmos;
using System.Text.Json.Serialization;
using Shopping.Domain.Core;

namespace Shopping.Infrastructure.Persistence.Cart;

public record CartService : IPersistenceIdentifier
{
    public string PartitionKey => CustomerId;
    
    public string Id { get; init; }

    public string CustomerId { get; init; }
    
    public DateTime CreatedOnUtc { get; init; }

    public Domain.Core.Persistence.Metadata Metadata { get; init; }
    
    public IEnumerable<CartItem> Items { get; init; }
    
    [JsonPropertyName("_etag")]
    public string ETag { get; init; }
    
}

public record CartItem(string Sku, uint Quantity);


public sealed class CartRepository: Repository<CartService>, IRepository<CartService>
{
    private const string ContainerName = "";
    private const string DatabaseName = "";

    public CartRepository(CosmosClient client) : base(client, DatabaseName, ContainerName) { }

    // public async Task BatchUpdateAsync(string partitionKey, Cart aggregate, IEnumerable<IEvent> events, CancellationToken cancellationToken)
    // {
    //     Shopping.Core.PartitionKey partitionKey = new (aggregate.PartitionKey);
    //     await base.BatchUpdateAsync(partitionKey, aggregate, events, cancellationToken);
    // }

    // public async Task BatchUpdateAsync(Cart aggregate, IEnumerable<IEvent> events, CancellationToken cancellationToken)
    // {
    //     await base.BatchUpdateAsync(aggregate, events, cancellationToken);
    // }
    public async Task BatchUpdateAsync(CartService aggregate, IEnumerable<IEvent> events, CancellationToken cancellationToken)
    {
        Domain.Core.PartitionKey partitionKey = new (aggregate.PartitionKey);
        await base.BatchUpdateAsync(partitionKey, aggregate, events, cancellationToken);
    }
}