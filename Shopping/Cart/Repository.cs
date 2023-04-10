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

public sealed class CartRepository: IRepository<Cart, IEvent>
{
    private const string Container = "";
    private const string Database = "";
    
    private readonly CosmosClient _client;

    public CartRepository(CosmosClient client)
    {
        _client = client;
    }

    public async Task<Cart> GetByIdAsync(string partitionKey, string id, CancellationToken cancellationToken)
    {
        var requestOptions = new ItemRequestOptions();
        var container = ConfigureContainer();
        ItemResponse<Cart> response = await container.ReadItemAsync<Cart>(id, new PartitionKey(partitionKey), requestOptions, cancellationToken);

        return response.Resource;
    }

    public async Task<IEnumerable<Cart>> GetByPartitionKeyAsync(string partitionKey, CancellationToken cancellationToken)
    {
        var container = ConfigureContainer();
        var requestOptions = new QueryRequestOptions
        {
            PartitionKey = new PartitionKey(partitionKey),
            MaxItemCount = 100
        };

        List<Cart> documents = new();
        QueryDefinition queryDefinition = new QueryDefinition("select * from c");
        using FeedIterator<Cart>? feedIterator = container.GetItemQueryIterator<Cart>(queryDefinition, "", requestOptions);
        while (feedIterator.HasMoreResults)
        {
            FeedResponse<Cart> items = await feedIterator.ReadNextAsync(cancellationToken);
            documents.AddRange(items);
        }

        return documents;
    }

    public async Task BatchUpdate(string partitionKey, Cart aggregate, IEnumerable<IEvent> events)
    {
        var container = ConfigureContainer();
        PartitionKey key = new PartitionKey(partitionKey);
        TransactionalBatch batch = container.CreateTransactionalBatch(key);

        batch.UpsertItem(aggregate);
        foreach (var @event in events)
        {
            batch.UpsertItem(@event);
        }
        
        using TransactionalBatchResponse response = await batch.ExecuteAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(response.ErrorMessage);
        }
    }

    private Container ConfigureContainer()
    {
        return _client.GetContainer(Database, Container);
    }
}