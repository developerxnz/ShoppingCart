using Microsoft.Azure.Cosmos;
using Shopping.Infrastructure.Interfaces;

namespace Shopping.Infrastructure.Persistence.Delivery;

public class Repository : Repository<Delivery>, IRepository<Delivery>
{
    private const string ContainerName = "Deliveries";
    private const string DatabaseName = "Shopping";
    
    public Repository(CosmosClient client) : base(client, DatabaseName, ContainerName) { }

    public async Task BatchUpdateAsync(Delivery aggregate, IEnumerable<Interfaces.IEvent> events, CancellationToken cancellationToken)
    {
        Domain.Core.PartitionKey partitionKey = new (aggregate.Id);
        await base.BatchUpdateAsync(partitionKey, aggregate, events, cancellationToken);
    }
}