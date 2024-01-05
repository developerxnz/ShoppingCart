using Microsoft.Azure.Cosmos;
using Shopping.Infrastructure.Interfaces;
using IEvent = Shopping.Infrastructure.Interfaces.IEvent;

namespace Shopping.Infrastructure.Persistence.Cart;

public sealed class Repository: Repository<Cart>, IRepository<Cart>
{
    private const string ContainerName = "Carts";
    private const string DatabaseName = "Shopping";

    public Repository(CosmosClient client) : base(client, DatabaseName, ContainerName) { }

    public async Task BatchUpdateAsync(Cart aggregate, IEnumerable<IEvent> events, CancellationToken cancellationToken)
    {
        Domain.Core.PartitionKey partitionKey = new (aggregate.Id);
        await base.BatchUpdateAsync(partitionKey, aggregate, events, cancellationToken);
    }
}