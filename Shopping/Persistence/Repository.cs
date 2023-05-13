using Microsoft.Azure.Cosmos;
using Shopping.Core;
using PartitionKey = Microsoft.Azure.Cosmos.PartitionKey;

namespace Shopping.Persistence;

public abstract class Repository<T>
{
    private readonly Container _container;

    protected Repository(CosmosClient client,string database, string containerName)
    {
        _container = client.GetContainer(database, containerName);
    }
    
    public async Task<T> GetByIdAsync(string partitionKey, string id, CancellationToken cancellationToken)
    {
        ItemRequestOptions requestOptions = new ItemRequestOptions();
        ItemResponse<T> response = await _container.ReadItemAsync<T>(id, new PartitionKey(partitionKey), requestOptions, cancellationToken);

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
        using FeedIterator<T>? feedIterator = _container.GetItemQueryIterator<T>(queryDefinition, "", requestOptions);
        while (feedIterator.HasMoreResults)
        {
            FeedResponse<T> items = await feedIterator.ReadNextAsync(cancellationToken);
            documents.AddRange(items);
        }

        return documents;
    }

    protected async Task BatchUpdateAsync(IPersistenceIdentifier aggregate, IEnumerable<object> events)
    {
        PartitionKey key = new PartitionKey(aggregate.PartitionKey);
        TransactionalBatch batch = _container.CreateTransactionalBatch(key);

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