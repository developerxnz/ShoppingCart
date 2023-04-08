using Shopping.Core;
using Shopping.Orders.Core;

namespace Shopping.Orders;

public record OrderAggregate : Aggregate<OrderAggregate>
{
    public OrderId Id { get; init; } = new(Guid.NewGuid());
    
    public DateTime? CancelledOnUtc { get; init; }
    
    public DateTime? CompletedOnUtc { get; init; }
    
    public DateTime CreatedOnUtc { get; init; }
    
    public CustomerId CustomerId { get; init; }

    public MetaData MetaData { get; init; }

    public OrderAggregate(DateTime createdOnUtc, CustomerId customerId)
    {
        Id = new OrderId(Guid.NewGuid());
        CreatedOnUtc = createdOnUtc;
        CustomerId = customerId;
        MetaData = new MetaData(new StreamId(Id.Value), new Shopping.Core.Version(0), createdOnUtc);
    }
    
}