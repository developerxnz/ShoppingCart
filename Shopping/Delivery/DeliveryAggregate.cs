using Shopping.Core;
using Shopping.Orders.Core;

namespace Shopping.Delivery.Core;

public interface IDeliveryAggregate
{
    DeliveryId Id { get; } 
    
    DateTime CreatedOnUtc { get; } 
    
    DateTime? DeliveredOnUtc { get; }
    
    MetaData MetaData { get; }
    
    OrderId OrderId { get; }
}

public sealed record DeliveryAggregate : Aggregate<DeliveryAggregate>, IAggregate
{
    public DeliveryId Id { get; init; } = new(Guid.NewGuid());
    
    public DateTime CreatedOnUtc { get; init; }
    
    public DateTime? DeliveredOnUtc { get; init; } = null;
    
    public MetaData MetaData { get; init; }
    
    public OrderId OrderId { get; init; }
    
    public DeliveryAggregate(DateTime createdOnUtc, OrderId orderId)
    {
        Id = new(Guid.NewGuid());
        CreatedOnUtc = createdOnUtc;
        MetaData = new MetaData(new StreamId(Id.Value), new Shopping.Core.Version(0), createdOnUtc);
        OrderId = orderId;
    }
};