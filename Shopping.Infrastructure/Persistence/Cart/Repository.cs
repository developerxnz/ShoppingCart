using Microsoft.Azure.Cosmos;
using Shopping.Domain.Core;
using Shopping.Infrastructure.Interfaces;
using IEvent = Shopping.Infrastructure.Interfaces.IEvent;

namespace Shopping.Infrastructure.Persistence.Cart;

public sealed class CartRepository: Repository<CartAggregate>, IRepository<CartAggregate>
{
    private const string ContainerName = "Cart";
    private const string DatabaseName = "Shopping";

    public CartRepository(CosmosClient client) : base(client, DatabaseName, ContainerName) { }

    // public async Task BatchUpdateAsync(string partitionKey, Cart aggregate, IEnumerable<IEvent> events, CancellationToken cancellationToken)
    // {
    //     Shopping.Core.PartitionKey partitionKey = new (aggregate.PartitionKey);
    //     await base.BatchUpdateAsync(partitionKey, aggregate, events, cancellationToken);
    // }

    // public async Task BatchUpdateAsync(Cart aggregate, IEnumerable<IEvent> events, CancellationToken cancellationToken)
    // {
    //     await base.BatchUpdateAsync(aggregate, events, cancellationToken);
    // }
    public async Task BatchUpdateAsync(CartAggregate aggregate, IEnumerable<CartEvent> events, CancellationToken cancellationToken)
    {
        Domain.Core.PartitionKey partitionKey = new (aggregate.PartitionKey);
        await base.BatchUpdateAsync(partitionKey, aggregate, events, cancellationToken);
    }

    public Task BatchUpdateAsync(CartAggregate aggregate, IEnumerable<IEvent> events, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}