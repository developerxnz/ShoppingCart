using Microsoft.Azure.Cosmos;
using Shopping.Infrastructure.Interfaces;

namespace Shopping.Infrastructure.Persistence.Products;

public class Repository : Repository<Product>, IRepository<Product>
{
    private const string Database = "Shopping";
    private const string ContainerName = "Products";
    
    public Repository(CosmosClient client) : base(client, Database, ContainerName) { }

    public async Task BatchUpdateAsync(Product aggregate, IEnumerable<Interfaces.IEvent> events, CancellationToken cancellationToken)
    {
        Domain.Core.PartitionKey partitionKey = new (aggregate.Id);
        await base.BatchUpdateAsync(partitionKey, aggregate, events, cancellationToken);
    }
}