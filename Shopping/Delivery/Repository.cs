using Microsoft.Azure.Cosmos;
using Shopping.Core;

namespace Shopping.Delivery.Persistence;

public sealed record Delivery(
    string Id,
    DateTime CreatedOnUtc,
    DateTime? DeliveredOnUtc,
    Shopping.Core.Persistence.Metadata Metadata,
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
        Shopping.Core.PartitionKey partitionKey = new (aggregate.PartitionKey);
        await base.BatchUpdateAsync(partitionKey, aggregate, events, cancellationToken);
    }
}