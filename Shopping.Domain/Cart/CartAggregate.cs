using Shopping.Domain.Cart.Core;
using Shopping.Domain.Core;
using Version = Shopping.Domain.Core.Version;

namespace Shopping.Domain.Cart;

public sealed record CartAggregate: Aggregate<CartAggregate>, IAggregate
{
    public CartId Id { get; init; } = new(Guid.NewGuid());
    
    public CustomerId CustomerId { get; init; }
    
    public DateTime CreatedOnUtc { get; init; }

    public MetaData MetaData { get; init; }
    
    public IEnumerable<CartItem> Items { get; init; }

    public string Etag { get; init; } = "";

    public CartAggregate(DateTime createdOnUtc, CustomerId customerId)
    {
        Id = new(Guid.NewGuid());
        CustomerId = customerId;
        Items = Enumerable.Empty<CartItem>();
        CreatedOnUtc = createdOnUtc;
        MetaData = new MetaData(new StreamId(Id.Value), new Version(0), createdOnUtc);
    }
}