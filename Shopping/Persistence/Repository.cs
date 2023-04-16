using Microsoft.Azure.Cosmos;

namespace Shopping.Persistence;

public abstract class Repository<T>
{
    private readonly CosmosClient _client;
    protected readonly Container Container;

    protected Repository(CosmosClient client,string database, string container)
    {
        _client = client;
        _client.GetContainer(database, container);
    }
    
    public async Task<T> GetByIdAsync(string partitionKey, string id, CancellationToken cancellationToken)
    {
        ItemRequestOptions requestOptions = new ItemRequestOptions();
        ItemResponse<T> response = await Container.ReadItemAsync<T>(id, new PartitionKey(partitionKey), requestOptions, cancellationToken);

        return response.Resource;
    }
    
    public async Task<IEnumerable<T>> GetByPartitionKeyAsync(string partitionKey, CancellationToken cancellationToken)
    {
        QueryRequestOptions requestOptions = new QueryRequestOptions
        {
            PartitionKey = new PartitionKey(partitionKey),
            MaxItemCount = 100
        };

        List<T> documents = new();
        QueryDefinition queryDefinition = new QueryDefinition("select * from c");
        using FeedIterator<T>? feedIterator = Container.GetItemQueryIterator<T>(queryDefinition, "", requestOptions);
        while (feedIterator.HasMoreResults)
        {
            FeedResponse<T> items = await feedIterator.ReadNextAsync(cancellationToken);
            documents.AddRange(items);
        }

        return documents;
    }

    protected async Task BatchUpdateAsync(string partitionKey, object aggregate, IEnumerable<object> events)
    {
        PartitionKey key = new PartitionKey(partitionKey);
        TransactionalBatch batch = Container.CreateTransactionalBatch(key);

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
}