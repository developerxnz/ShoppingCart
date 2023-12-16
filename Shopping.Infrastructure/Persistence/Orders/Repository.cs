using Microsoft.Azure.Cosmos;
using Shopping.Core;
using Metadata = Shopping.Core.Persistence.Metadata;
using PartitionKey = Shopping.Core.PartitionKey;

namespace Shopping.Infrastructure.Persistence.Orders;

public record Order(
    string Id,
    DateTime? CancelledOnUtc,
    DateTime? CompletedOnUtc,
    DateTime CreatedOnUtc,
    string CustomerId,
    Metadata MetaData) : IPersistenceIdentifier
{
    public string PartitionKey => CustomerId;
}

public class Repository : Repository<Order>, IRepository<Order>
{
    public Repository(CosmosClient client, string database, string container) : base(client, database, container)
    {
    }

    public async Task BatchUpdateAsync(Order aggregate, IEnumerable<IEvent> events, CancellationToken cancellationToken)
    {
        PartitionKey partitionKey = new PartitionKey(aggregate.PartitionKey);
        await base.BatchUpdateAsync(partitionKey, aggregate, events, cancellationToken);
    }
}