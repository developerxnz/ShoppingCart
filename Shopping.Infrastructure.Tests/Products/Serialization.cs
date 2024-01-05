using System.Text.Json;
using Shopping.Infrastructure.Persistence.Products;

namespace Shopping.Infrastructure.Tests.Products;

public class Serialization
{
    [Fact]
    public void Serialize_For_ProductCreatedEvent_Should_Return_Expected_Value()
    {
        string id = Guid.NewGuid().ToString();
        DateTime timestamp = DateTime.UtcNow;
        string sku = Guid.NewGuid().ToString();
        uint version = 5u;
        string correlationId = Guid.NewGuid().ToString();
        string causationId = Guid.NewGuid().ToString();
        string description = Guid.NewGuid().ToString();
        decimal amount = 100.10m;

        ProductCreatedEvent @event = new ProductCreatedEvent(
            id,
            sku,
            description,
            amount,
            timestamp,
            version,
            correlationId,
            causationId
        );

        string json = JsonSerializer.Serialize<ProductEvent>(@event);
        ProductEvent? deserializedEvent = JsonSerializer.Deserialize<ProductEvent>(json);

        switch (deserializedEvent)
        {
            case ProductCreatedEvent productCreatedEvent:
                Assert.Equal(@event, productCreatedEvent);
                break;
            default:
                Assert.Fail($"Expected {nameof(ProductCreatedEvent)}");
                break;
        }
    }
    
    [Fact]
    public void Serialize_For_ProductUpdatedEvent_Should_Return_Expected_Value()
    {
        string id = Guid.NewGuid().ToString();
        DateTime timestamp = DateTime.UtcNow;
        string sku = Guid.NewGuid().ToString();
        uint version = 5u;
        string correlationId = Guid.NewGuid().ToString();
        string causationId = Guid.NewGuid().ToString();
        string description = Guid.NewGuid().ToString();
        decimal amount = 100.10m;

        ProductUpdatedEvent @event = new ProductUpdatedEvent(
            id,
            sku,
            description,
            amount,
            timestamp,
            version,
            correlationId,
            causationId
        );

        string json = JsonSerializer.Serialize<ProductEvent>(@event);
        ProductEvent? deserializedEvent = JsonSerializer.Deserialize<ProductEvent>(json);

        switch (deserializedEvent)
        {
            case ProductUpdatedEvent productUpdatedEvent:
                Assert.Equal(@event, productUpdatedEvent);
                break;
            default:
                Assert.Fail($"Expected {nameof(ProductUpdatedEvent)}");
                break;
        }
    }
}