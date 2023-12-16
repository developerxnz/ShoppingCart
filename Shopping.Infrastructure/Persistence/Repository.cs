using Microsoft.Azure.Cosmos;
using PartitionKey = Microsoft.Azure.Cosmos.PartitionKey;

namespace Shopping.Infrastructure.Persistence;

public abstract class Repository<T>
{
    protected readonly Container Container;

    protected Repository(CosmosClient client,string database, string containerName)
    {
        Container = client.GetContainer(database, containerName);
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

    protected async Task BatchUpdateAsync(Domain.Core.PartitionKey partitionKey, object aggregate, IEnumerable<object> events, CancellationToken cancellationToken)
    {
        try
        {
            TransactionalBatch batch = Container.CreateTransactionalBatch(new PartitionKey(partitionKey.Value));
            batch.UpsertItem(aggregate);
            
            foreach (var @event in events)
            {
                batch.UpsertItem(@event);
            }
        
            using TransactionalBatchResponse response = await batch.ExecuteAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("failed");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}