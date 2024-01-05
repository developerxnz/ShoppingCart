using Microsoft.Azure.Cosmos;
using Shopping.Infrastructure.Interfaces;
using PartitionKey = Shopping.Domain.Core.PartitionKey;

namespace Shopping.Infrastructure.Persistence.Orders;

public class Repository : Repository<Order>, IRepository<Order>
{
    
    private const string ContainerName = "Orders";
    private const string DatabaseName = "Shopping";
    
    public Repository(CosmosClient client) : base(client, DatabaseName, ContainerName)
    {
    }

    public async Task BatchUpdateAsync(Order aggregate, IEnumerable<Interfaces.IEvent> events, CancellationToken cancellationToken)
    {
        PartitionKey partitionKey = new PartitionKey(aggregate.Id);
        await base.BatchUpdateAsync(partitionKey, aggregate, events, cancellationToken);
    }
}