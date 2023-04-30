using Microsoft.Azure.Cosmos;
using Shopping.Core;
using MetaData = Shopping.Core.Persistence.MetaData;

//using Shopping.Core;

namespace Shopping.Orders.Persistence;

public record Order(
    string Id,
    DateTime? CancelledOnUtc,
    DateTime? CompletedOnUtc,
    DateTime CreatedOnUtc,
    string CustomerId,
    MetaData MetaData);

public class Repository : Shopping.Persistence.Repository<Order>, IRepository<Order>
{
    private const string ContainerName = "";
    private const string DatabaseName = "";
    
    public Repository(CosmosClient client, string database, string container) : base(client, database, container) { }

    public async Task BatchUpdateAsync(string partitionKey, Order aggregate, IEnumerable<IEvent> events)
    {
        await base.BatchUpdateAsync(partitionKey, aggregate, events);
    }
}