using Microsoft.Azure.Cosmos;
using Shopping.Infrastructure.Interfaces;
using PartitionKey = Shopping.Domain.Core.PartitionKey;

namespace Shopping.Infrastructure.Persistence.Orders;

public class Repository : Repository<Order>, IRepository<Order>
{
    public Repository(CosmosClient client, string database, string container) : base(client, database, container)
    {
    }

    public async Task BatchUpdateAsync(Order aggregate, IEnumerable<Interfaces.IEvent> events, CancellationToken cancellationToken)
    {
        PartitionKey partitionKey = new PartitionKey(aggregate.PartitionKey);
        await base.BatchUpdateAsync(partitionKey, aggregate, events, cancellationToken);
    }
}