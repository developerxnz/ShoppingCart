using System.Text.Json;
using Shopping.Infrastructure.Persistence.Delivery;

namespace Shopping.Infrastructure.Tests.Delivery;

public class Serialization
{
    [Fact]
    public void Serialize_For_DeliveryCreatedEvent_Should_Return_Expected_Value()
    {
        string id = Guid.NewGuid().ToString();
        string customerId = Guid.NewGuid().ToString();
        DateTime timestamp = DateTime.UtcNow;
        string sku = Guid.NewGuid().ToString();
        uint quantity = 100;
        uint version = 5u;
        string correlationId = Guid.NewGuid().ToString();
        string causationId = Guid.NewGuid().ToString();

        DeliveryCreatedEvent @event = new DeliveryCreatedEvent(
            timestamp,
            customerId,
            id,
            version,
            correlationId,
            causationId
        );
        
        string json = JsonSerializer.Serialize<DeliveryEvent>(@event);
        DeliveryEvent? deserializedEvent = JsonSerializer.Deserialize<DeliveryEvent>(json);
        
        switch (deserializedEvent)
        {
            case DeliveryCreatedEvent deliveryCreatedEvent:
                Assert.Equal(@event, deliveryCreatedEvent);
                break;
            default:
                Assert.Fail($"Expected {nameof(DeliveryCreatedEvent)}");
                break;
        }
    }
    
    [Fact]
    public void Serialize_For_DeliveryCompletedEvent_Should_Return_Expected_Value()
    {
        string id = Guid.NewGuid().ToString();
        string orderId = Guid.NewGuid().ToString();
        string customerId = Guid.NewGuid().ToString();
        DateTime timestamp = DateTime.UtcNow;
        string sku = Guid.NewGuid().ToString();
        uint quantity = 100;
        uint version = 5u;
        string correlationId = Guid.NewGuid().ToString();
        string causationId = Guid.NewGuid().ToString();

        DeliveryCompletedEvent @event = new DeliveryCompletedEvent(
            timestamp,
            customerId,
            id,
            orderId,
            version,
            correlationId,
            causationId
        );
        
        string json = JsonSerializer.Serialize<DeliveryEvent>(@event);
        DeliveryEvent? deserializedEvent = JsonSerializer.Deserialize<DeliveryEvent>(json);
        
        switch (deserializedEvent)
        {
            case DeliveryCompletedEvent deliveryCompletedEvent:
                Assert.Equal(@event, deliveryCompletedEvent);
                break;
            default:
                Assert.Fail($"Expected {nameof(DeliveryCompletedEvent)}");
                break;
        }
    }
    
    [Fact]
    public void Serialize_For_DeliveryCancelledEvent_Should_Return_Expected_Value()
    {
        string id = Guid.NewGuid().ToString();
        string orderId = Guid.NewGuid().ToString();
        string customerId = Guid.NewGuid().ToString();
        DateTime timestamp = DateTime.UtcNow;
        string sku = Guid.NewGuid().ToString();
        uint quantity = 100;
        uint version = 5u;
        string correlationId = Guid.NewGuid().ToString();
        string causationId = Guid.NewGuid().ToString();

        DeliveryCancelledEvent @event = new DeliveryCancelledEvent(
            timestamp,
            customerId,
            id,
            orderId,
            version,
            correlationId,
            causationId
        );
        
        string json = JsonSerializer.Serialize<DeliveryEvent>(@event);
        DeliveryEvent? deserializedEvent = JsonSerializer.Deserialize<DeliveryEvent>(json);
        
        switch (deserializedEvent)
        {
            case DeliveryCancelledEvent deliveryCancelledEvent:
                Assert.Equal(@event, deliveryCancelledEvent);
                break;
            default:
                Assert.Fail($"Expected {nameof(DeliveryCancelledEvent)}");
                break;
        }
    }
    
}