using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Shopping.Core;

namespace Shopping.Cart.Persistence;

public record MetaData(string StreamId, uint Version, DateTime TimeStamp);

public record Cart
{
    public string Id { get; init; }

    public string CustomerId { get; init; }
    
    public DateTime CreatedOnUtc { get; init; }

    public MetaData MetaData { get; init; }
    
    public IEnumerable<CartItem> Items { get; init; }
    
    [JsonProperty("_etag")]
    public string ETag { get; init; }
    
}

public record CartItem(string Sku, uint Quantity);


public sealed class CartRepository: Shopping.Persistence.Repository<Cart>, IRepository<Cart>
{
    private const string ContainerName = "";
    private const string DatabaseName = "";

    public CartRepository(CosmosClient client) : base(client, DatabaseName, ContainerName) { }

    public async Task BatchUpdateAsync(string partitionKey, Cart aggregate, IEnumerable<IEvent> events)
    {
        await base.BatchUpdateAsync(partitionKey, aggregate, events);
    }
}