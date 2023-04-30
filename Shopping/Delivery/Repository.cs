using Microsoft.Azure.Cosmos;
using Shopping.Core;

namespace Shopping.Delivery.Persistence;

public sealed record Delivery(
    string Id,
    DateTime CreatedOnUtc,
    DateTime? DeliveredOnUtc,
    Shopping.Core.Persistence.MetaData MetaData,
    string OrderId
);

public class Repository : Shopping.Persistence.Repository<Delivery>, IRepository<Delivery>
{
    private const string ContainerName = "";
    private const string DatabaseName = "";
    
    public Repository(CosmosClient client, string database, string container) : base(client, database, container) { }

    public async Task BatchUpdateAsync(string partitionKey, Delivery aggregate, IEnumerable<IEvent> events)
    {
        await base.BatchUpdateAsync(partitionKey, aggregate, events);
    }
}