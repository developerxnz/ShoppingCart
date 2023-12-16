using Shopping.Domain.Core;
using Shopping.Domain.Delivery.Core;
using Shopping.Domain.Orders.Core;

namespace ShoppingUnitTests.Delivery;

public class DeliveryTransformerTests
{
    private readonly ITransformer<DeliveryAggregate, Shopping.Infrastructure.Persistence.Delivery.Delivery> _transformer;

    public DeliveryTransformerTests()
    {
        _transformer = new Shopping.Services.Delivery.DeliveryTransformer();
    }

    [Fact]
    public void FromDomain_Should_Return_Expected_Dto()
    {
        OrderId orderId = OrderId.Create();
        DateTime createdOnUtc = DateTime.UtcNow;
        CustomerId customerId = new CustomerId(Guid.NewGuid());

        DeliveryAggregate aggregate = new DeliveryAggregate(createdOnUtc, orderId);

        var deliveryDto = _transformer.FromDomain(aggregate);

        Assert.Equal(orderId.Value.ToString(), deliveryDto.OrderId);
        Assert.Equal(createdOnUtc, deliveryDto.CreatedOnUtc);
    }

    [Fact]
    public void ToDomain_Should_Return_Expected_Domain()
    {
        DeliveryId deliveryId = new DeliveryId(Guid.NewGuid());
        Guid orderId = Guid.NewGuid();
        DateTime createdOnUtc = DateTime.UtcNow;
        DateTime deliveredOnUtc = DateTime.UtcNow;
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        Guid streamId = Guid.NewGuid();
        uint version = 11;

        Shopping.Infrastructure.Persistence.Delivery.Delivery dto =
            new Shopping.Infrastructure.Persistence.Delivery.Delivery(
                deliveryId.Value.ToString(),
                createdOnUtc,
                deliveredOnUtc,
                new Shopping.Domain.Core.Persistence.Metadata(streamId.ToString(), version, createdOnUtc), orderId.ToString());

        _transformer.ToDomain(dto)
            .Switch(
                aggregate =>
                {
                    Assert.Equal(createdOnUtc, aggregate.CreatedOnUtc);
                    Assert.Equal(deliveredOnUtc, aggregate.DeliveredOnUtc);
                    Assert.Equal(deliveryId, aggregate.Id);
                    Assert.Equal(orderId, aggregate.OrderId.Value);
                    
                    Assert.Equal(streamId, aggregate.MetaData.StreamId.Value);
                    Assert.Equal(version, aggregate.MetaData.Version.Value);
                    Assert.Equal(createdOnUtc, aggregate.MetaData.TimeStamp);
                },
                onError => Assert.Fail($"Expected {nameof(DeliveryAggregate)}")
            );
    }
}