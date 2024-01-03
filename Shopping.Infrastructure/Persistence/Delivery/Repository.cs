using Microsoft.Azure.Cosmos;
using Shopping.Infrastructure.Interfaces;

namespace Shopping.Infrastructure.Persistence.Delivery;

public class Repository : Repository<Delivery>, IRepository<Delivery>
{
    public Repository(CosmosClient client, string database, string container) : base(client, database, container) { }

    public async Task BatchUpdateAsync(Delivery aggregate, IEnumerable<Interfaces.IEvent> events, CancellationToken cancellationToken)
    {
        Domain.Core.PartitionKey partitionKey = new (aggregate.PartitionKey);
        await base.BatchUpdateAsync(partitionKey, aggregate, events, cancellationToken);
    }
}