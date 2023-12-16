using Microsoft.Azure.Cosmos;
using Shopping.Domain.Core;

namespace Shopping.Infrastructure.Persistence.Delivery;

public sealed record Delivery(
    string Id,
    DateTime CreatedOnUtc,
    DateTime? DeliveredOnUtc,
    Domain.Core.Persistence.Metadata Metadata,
    string OrderId
): IPersistenceIdentifier
{
    public string PartitionKey => OrderId;
}

public class Repository : Repository<Delivery>, IRepository<Delivery>
{
    public Repository(CosmosClient client, string database, string container) : base(client, database, container) { }

    public async Task BatchUpdateAsync(Delivery aggregate, IEnumerable<IEvent> events, CancellationToken cancellationToken)
    {
        Domain.Core.PartitionKey partitionKey = new (aggregate.PartitionKey);
        await base.BatchUpdateAsync(partitionKey, aggregate, events, cancellationToken);
    }
}