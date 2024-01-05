using System.Text.Json;
using Shopping.Infrastructure.Persistence.Cart;

namespace Shopping.Infrastructure.Tests.Cart;

public class Serialization
{
    [Fact]
    public void Serialize_For_CartItemAddedEvent_Should_Return_Expected_Value()
    {
        string id = Guid.NewGuid().ToString();
        string customerId = Guid.NewGuid().ToString();
        DateTime timestamp = DateTime.UtcNow;
        string sku = Guid.NewGuid().ToString();
        uint quantity = 100;
        uint version = 5u;
        string correlationId = Guid.NewGuid().ToString();
        string causationId = Guid.NewGuid().ToString();

        CartItemAddedEvent @event = new CartItemAddedEvent(
            id,
            customerId,
            timestamp,
            sku,
            quantity,
            version,
            correlationId,
            causationId
        );
        
        string json = JsonSerializer.Serialize<CartEvent>(@event);
        CartEvent? deserializedEvent = JsonSerializer.Deserialize<CartEvent>(json);
        
        switch (deserializedEvent)
        {
            case CartItemAddedEvent cartItemAddedEvent:
                Assert.Equal(@event, deserializedEvent);
                break;
            default:
                Assert.Fail($"Expected {nameof(CartItemAddedEvent)}");
                break;
        }
    }
    
    [Fact]
    public void Serialize_For_CartItemRemovedEvent_Should_Return_Expected_Value()
    {
        string id = Guid.NewGuid().ToString();
        string customerId = Guid.NewGuid().ToString();
        DateTime timestamp = DateTime.UtcNow;
        string sku = Guid.NewGuid().ToString();
        uint quantity = 100;
        uint version = 5u;
        string correlationId = Guid.NewGuid().ToString();
        string causationId = Guid.NewGuid().ToString();

        CartItemRemovedEvent @event = new CartItemRemovedEvent(
            id,
            customerId,
            timestamp,
            sku,
            version,
            correlationId,
            causationId
        );
        
        string json = JsonSerializer.Serialize<CartEvent>(@event);
        CartEvent? deserializedEvent = JsonSerializer.Deserialize<CartEvent>(json);
        switch (deserializedEvent)
        {
            case CartItemRemovedEvent cartItemRemovedEvent:
                Assert.Equal(@event, deserializedEvent);
                break;
            default:
                Assert.Fail($"Expected {nameof(CartItemAddedEvent)}");
                break;
        }
    }
    
    [Fact]
    public void Serialize_For_CartItemUpdatedEvent_Should_Return_Expected_Value()
    {
        string id = Guid.NewGuid().ToString();
        string customerId = Guid.NewGuid().ToString();
        DateTime timestamp = DateTime.UtcNow;
        string sku = Guid.NewGuid().ToString();
        uint quantity = 100;
        uint version = 5u;
        string correlationId = Guid.NewGuid().ToString();
        string causationId = Guid.NewGuid().ToString();

        CartItemUpdatedEvent @event = new CartItemUpdatedEvent(
            id,
            customerId,
            timestamp,
            sku,
            quantity,
            version,
            correlationId,
            causationId
        );
        
        string json = JsonSerializer.Serialize<CartEvent>(@event);
        CartEvent? deserializedEvent = JsonSerializer.Deserialize<CartEvent>(json);
        
        switch (deserializedEvent)
        {
            case CartItemUpdatedEvent cartItemUpdatedEvent:
                Assert.Equal(@event, deserializedEvent);
                break;
            default:
                Assert.Fail($"Expected {nameof(CartItemAddedEvent)}");
                break;
        }
    }
    
}