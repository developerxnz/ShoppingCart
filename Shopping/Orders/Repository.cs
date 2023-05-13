using Microsoft.Azure.Cosmos;
using Shopping.Core;
using MetaData = Shopping.Core.Persistence.MetaData;

namespace Shopping.Orders.Persistence;

public record Order(
    string Id,
    DateTime? CancelledOnUtc,
    DateTime? CompletedOnUtc,
    DateTime CreatedOnUtc,
    string CustomerId,
    MetaData MetaData): IPersistenceIdentifier
{
    public string PartitionKey => CustomerId;
}

public class Repository : Shopping.Persistence.Repository<Order>, IRepository<Order>
{
    public Repository(CosmosClient client, string database, string container) : base(client, database, container) { }

    public async Task BatchUpdateAsync(Order aggregate, IEnumerable<IEvent> events, CancellationToken cancellationToken)
    {
        await base.BatchUpdateAsync(aggregate, events);
    }
}