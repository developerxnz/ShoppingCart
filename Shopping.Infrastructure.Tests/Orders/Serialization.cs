using System.Text.Json;
using Shopping.Infrastructure.Persistence.Orders.Events;

namespace Shopping.Infrastructure.Tests.Orders;

public class Serialization
{
    [Fact]
    public void Serialize_For_OrderCreatedEvent_Should_Return_Expected_Value()
    {
        string id = Guid.NewGuid().ToString();
        string customerId = Guid.NewGuid().ToString();
        DateTime timestamp = DateTime.UtcNow;
        uint version = 5u;
        string correlationId = Guid.NewGuid().ToString();
        string causationId = Guid.NewGuid().ToString();

        OrderCreatedEvent @event = new OrderCreatedEvent(
            id,
            timestamp,
            customerId,
            version,
            correlationId,
            causationId
        );
        
        string json = JsonSerializer.Serialize<OrderEvent>(@event);
        OrderEvent? deserializedEvent = JsonSerializer.Deserialize<OrderEvent>(json);
        
        switch (deserializedEvent)
        {
            case OrderCreatedEvent orderCreatedEvent:
                Assert.Equal(@event, deserializedEvent);
                break;
            default:
                Assert.Fail($"Expected {nameof(OrderCreatedEvent)}");
                break;
        }
    }
    
    
    [Fact]
    public void Serialize_For_OrderCancelledEvent_Should_Return_Expected_Value()
    {
        string id = Guid.NewGuid().ToString();
        string customerId = Guid.NewGuid().ToString();
        string orderId = Guid.NewGuid().ToString();
        DateTime timestamp = DateTime.UtcNow;
        uint version = 5u;
        string correlationId = Guid.NewGuid().ToString();
        string causationId = Guid.NewGuid().ToString();

        OrderCancelledEvent @event = new OrderCancelledEvent(
            id,
            timestamp,
            customerId,
            orderId,
            version,
            correlationId,
            causationId
        );
        
        string json = JsonSerializer.Serialize<OrderEvent>(@event);
        OrderEvent? deserializedEvent = JsonSerializer.Deserialize<OrderEvent>(json);
        
        switch (deserializedEvent)
        {
            case OrderCancelledEvent orderCancelledEvent:
                Assert.Equal(@event, orderCancelledEvent);
                break;
            default:
                Assert.Fail($"Expected {nameof(OrderCancelledEvent)}");
                break;
        }
    }
    
    [Fact]
    public void Serialize_For_OrderCompletedEvent_Should_Return_Expected_Value()
    {
        string id = Guid.NewGuid().ToString();
        string customerId = Guid.NewGuid().ToString();
        string orderId = Guid.NewGuid().ToString();
        DateTime timestamp = DateTime.UtcNow;
        uint version = 5u;
        string correlationId = Guid.NewGuid().ToString();
        string causationId = Guid.NewGuid().ToString();

        OrderCompletedEvent @event = new OrderCompletedEvent(
            id,
            timestamp,
            customerId,
            orderId,
            version,
            correlationId,
            causationId
        );
        
        string json = JsonSerializer.Serialize<OrderEvent>(@event);
        OrderEvent? deserializedEvent = JsonSerializer.Deserialize<OrderEvent>(json);
        
        switch (deserializedEvent)
        {
            case OrderCompletedEvent orderCompletedEvent:
                Assert.Equal(@event, orderCompletedEvent);
                break;
            default:
                Assert.Fail($"Expected {nameof(OrderCompletedEvent)}");
                break;
        }
    }
    
}