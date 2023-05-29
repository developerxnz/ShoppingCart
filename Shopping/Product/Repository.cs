using Microsoft.Azure.Cosmos;
using Shopping.Core;

namespace Shopping.Product.Persistence;

public record Product(
    string Id,
    string Sku,
    string Description,
    decimal Amount,
    DateTime CreatedOnUtc,
    MetaData MetaData): IPersistenceIdentifier
{
    public string PartitionKey => Id;
}

public class Repository : Shopping.Persistence.Repository<Product>, IRepository<Product>
{
    public Repository(CosmosClient client, string database, string containerName) 
        : base(client, database, containerName) { }

    public async Task BatchUpdateAsync(Product aggregate, IEnumerable<IEvent> events, CancellationToken cancellationToken)
    {
        await base.BatchUpdateAsync(aggregate, events);
    }
}