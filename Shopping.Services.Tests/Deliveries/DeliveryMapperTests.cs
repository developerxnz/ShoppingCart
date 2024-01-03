using Shopping.Domain.Core;
using Shopping.Domain.Delivery.Core;
using Shopping.Domain.Delivery.Events;
using Shopping.Domain.Orders.Core;
using Shopping.Services.Interfaces;
using DeliveryEvent = Shopping.Infrastructure.Persistence.Delivery.DeliveryEvent;

namespace ShoppingUnitTests.Delivery;

public class DeliveryMapperTests
{
    private readonly IMapper<DeliveryAggregate, Shopping.Infrastructure.Persistence.Delivery.Delivery, IDeliveryEvent,
        DeliveryEvent> _mapper;

    public DeliveryMapperTests()
    {
        _mapper = new Shopping.Services.Delivery.DeliveryMapper();
    }

    [Fact]
    public void FromDomain_Should_Return_Expected_Dto()
    {
        OrderId orderId = OrderId.Create();
        DateTime createdOnUtc = DateTime.UtcNow;
        CustomerId customerId = new CustomerId(Guid.NewGuid());

        DeliveryAggregate aggregate = new DeliveryAggregate(createdOnUtc, orderId);

        var deliveryDto = _mapper.FromDomain(aggregate);

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
            new Shopping.Infrastructure.Persistence.Delivery.Delivery
            {
                Id = deliveryId.Value.ToString(),
                CreatedOnUtc = createdOnUtc,
                DeliveredOnUtc = deliveredOnUtc,
                Metadata = new Shopping.Domain.Core.Persistence.Metadata(streamId.ToString(), version, createdOnUtc),
                OrderId = orderId.ToString()
            };

        _mapper.ToDomain(dto)
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