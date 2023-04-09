using Shopping.Core;
using Shopping.Orders.Core;

namespace Shopping.Orders;

public enum OrderStatus
{
    Cancelled,
    Completed,
    Pending
}

public record OrderAggregate : Aggregate<OrderAggregate>
{
    public OrderId Id { get; init; } = new(Guid.NewGuid());
    
    public DateTime? CancelledOnUtc { get; init; }
    
    public DateTime? CompletedOnUtc { get; init; }
    
    public DateTime CreatedOnUtc { get; init; }
    
    public CustomerId CustomerId { get; init; }

    public MetaData MetaData { get; init; }

    public OrderStatus OrderStatus()
    {
        if (CancelledOnUtc.HasValue)
        {
            return Orders.OrderStatus.Cancelled;
        }

        if (CompletedOnUtc.HasValue)
        {
            return Orders.OrderStatus.Completed;
        }

        return Orders.OrderStatus.Pending;
    }
        

    public OrderAggregate(DateTime createdOnUtc, CustomerId customerId)
    {
        Id = new OrderId(Guid.NewGuid());
        CustomerId = customerId;
        CreatedOnUtc = createdOnUtc;
        MetaData = new MetaData(new StreamId(Id.Value), new Shopping.Core.Version(0), createdOnUtc);
    }
}