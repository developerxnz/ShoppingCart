using Microsoft.Azure.Cosmos;
using Shopping.Core;
using Shopping.Core.Persistence;

namespace Shopping.Infrastructure.Persistence.Products;

public record Product(
    string Id,
    
    string Sku,
    
    string Description,
    
    decimal Amount,
    
    DateTime CreatedOnUtc,
    
    Metadata Metadata): IPersistenceIdentifier
{
    public string PartitionKey => Id;
}

public record ProductCreatedData(string Id, string Sku, string Description, decimal Amount, DateTime CreatedOnUtc)
{
    public string Type => "ProductCreated";
};

public record ProductUpdatedData(string Id, string Sku, string Description, decimal Amount, DateTime CreatedOnUtc)
{
    public string Type => "ProductUpdated";
};

public record EventMetadata(string Id, string CorrelationId, string CausationId);

public record ProductCreatedEventDto(ProductCreatedData Data, EventMetadata Event, Metadata Metadata);


public class Repository : Repository<Product>, IRepository<Product>
{
    private const string Database = "Shopping";
    private const string ContainerName = "Products";
    
    public Repository(CosmosClient client) 
        : base(client, Database, ContainerName) { }

    public async Task BatchUpdateAsync(Product aggregate, IEnumerable<IEvent> events, CancellationToken cancellationToken)
    {
        Shopping.Core.PartitionKey partitionKey = new (aggregate.PartitionKey);
        await base.BatchUpdateAsync(partitionKey, aggregate, events, cancellationToken);
    }
}