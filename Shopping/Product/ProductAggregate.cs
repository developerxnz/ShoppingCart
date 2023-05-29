using Shopping.Cart.Projections;
using Shopping.Core;

namespace Shopping.Product;

public sealed record ProductAggregate : Aggregate<ProductAggregate>, IAggregate
{
    public ProductId Id { get; init; } = new(Guid.NewGuid());
    
    public ProductDescription Description { get; init; }
    
    public ProductPrice Price { get; init; }
    
    public Sku Sku { get; init; }
    
    public DateTime CreatedOnUtc { get; init; }
    
    public MetaData MetaData { get; }
    
    public ProductAggregate(DateTime createdOnUtc)
    {
        Id = new ProductId(Guid.NewGuid());
        CreatedOnUtc = createdOnUtc;
        MetaData = new MetaData(new StreamId(Id.Value), new Core.Version(0), createdOnUtc);
    }
}