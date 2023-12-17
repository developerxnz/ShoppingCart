using Shopping.Domain.Core;
using Shopping.Domain.Product.Core;
using Version = Shopping.Domain.Core.Version;

namespace Shopping.Domain.Product;

public sealed record ProductAggregate : Aggregate<ProductAggregate>, IAggregate
{
    public ProductId Id { get; init; } = new(Guid.NewGuid());
    
    public ProductDescription Description { get; init; }
    
    public ProductPrice Price { get; init; }
    
    public Sku Sku { get; init; }
    
    public DateTime CreatedOnUtc { get; init; }
    
    //public DateTime UpdatedOnUtc { get; init; }
    
    public MetaData MetaData { get; init; }
    
    public ProductAggregate(DateTime createdOnUtc)
    {
        Id = new ProductId(Guid.NewGuid());
        CreatedOnUtc = createdOnUtc;
        //UpdatedOnUtc = createdOnUtc;
        MetaData = new MetaData(new StreamId(Id.Value), new Version(0), createdOnUtc);
    }
}