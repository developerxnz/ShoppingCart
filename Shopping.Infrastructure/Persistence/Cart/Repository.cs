using Microsoft.Azure.Cosmos;
using Shopping.Infrastructure.Interfaces;
using IEvent = Shopping.Infrastructure.Interfaces.IEvent;

namespace Shopping.Infrastructure.Persistence.Cart;

public sealed class Repository: Repository<CartAggregate>, IRepository<CartAggregate>
{
    private const string ContainerName = "Carts";
    private const string DatabaseName = "Shopping";

    public Repository(CosmosClient client) : base(client, DatabaseName, ContainerName) { }

    public async Task BatchUpdateAsync(CartAggregate aggregate, IEnumerable<IEvent> events, CancellationToken cancellationToken)
    {
        Domain.Core.PartitionKey partitionKey = new (aggregate.PartitionKey);
        await base.BatchUpdateAsync(partitionKey, aggregate, events, cancellationToken);
    }
}