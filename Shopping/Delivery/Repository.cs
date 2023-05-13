using Microsoft.Azure.Cosmos;
using Shopping.Core;
using PartitionKey = Shopping.Core.PartitionKey;

namespace Shopping.Delivery.Persistence;

public sealed record Delivery(
    string Id,
    DateTime CreatedOnUtc,
    DateTime? DeliveredOnUtc,
    Shopping.Core.Persistence.MetaData MetaData,
    string OrderId
): IPersistenceIdentifier
{
    public string PartitionKey => OrderId;
}

public class Repository : Shopping.Persistence.Repository<Delivery>, IRepository<Delivery>
{
    public Repository(CosmosClient client, string database, string container) : base(client, database, container) { }

    public async Task BatchUpdateAsync(Delivery aggregate, IEnumerable<IEvent> events, CancellationToken cancellationToken)
    {
        await base.BatchUpdateAsync(aggregate, events);
    }
}