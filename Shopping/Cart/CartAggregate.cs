using Shopping.Core;

namespace Shopping.Cart;

public sealed record CartAggregate: Aggregate<CartAggregate>, IAggregate
{
    public CartId Id { get; init; } = new(Guid.NewGuid());

    public DateTime CreatedOnUtc { get; init; }

    public MetaData MetaData { get; init; }
    
    public IEnumerable<CartItem> Items { get; init; }

    public CartAggregate(DateTime createdOnUtc)
    {
        Id = new(Guid.NewGuid());
        Items = Enumerable.Empty<CartItem>();
        CreatedOnUtc = createdOnUtc;
        MetaData = new MetaData(new StreamId(Id.Value), new Shopping.Core.Version(0), createdOnUtc);
    }
}