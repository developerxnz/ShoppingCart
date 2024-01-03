using ErrorOr;
using Shopping.Domain.Core;
using Shopping.Domain.Core.Persistence;
using Shopping.Domain.Orders;
using Shopping.Domain.Orders.Core;
using Shopping.Domain.Orders.Events;
using Shopping.Infrastructure.Persistence.Orders;
using Shopping.Services.Interfaces;
using OrderCancelledEvent = Shopping.Domain.Orders.Events.OrderCancelledEvent;
using OrderCompletedEvent = Shopping.Domain.Orders.Events.OrderCompletedEvent;
using OrderCreatedEvent = Shopping.Domain.Orders.Events.OrderCreatedEvent;
using OrderEvent = Shopping.Infrastructure.Persistence.Orders.OrderEvent;

namespace Shopping.Services.Orders;

public class OrderMapper :
    Mapper<OrderAggregate, Infrastructure.Persistence.Orders.Order, IOrderEvent, OrderEvent>,
    IMapper<OrderAggregate, Infrastructure.Persistence.Orders.Order, IOrderEvent, OrderEvent>
{
    public override Infrastructure.Persistence.Orders.Order FromDomain(OrderAggregate aggregate)
    {
        return new Infrastructure.Persistence.Orders.Order
        {
            CancelledOnUtc = aggregate.CancelledOnUtc,
            CompletedOnUtc = aggregate.CompletedOnUtc,
            CreatedOnUtc = aggregate.CreatedOnUtc,
            CustomerId = aggregate.CustomerId.Value.ToString(),
            Metadata = new Metadata(
                aggregate.MetaData.StreamId.Value.ToString(),
                aggregate.MetaData.Version.Value,
                aggregate.MetaData.TimeStamp)
        };
    }

    public override OrderEvent FromDomain(IOrderEvent @event)
        => @event switch
        {
            OrderCompletedEvent orderCompletedEvent => orderCompletedEvent.FromDomain(),
            OrderCreatedEvent orderCreatedEvent => orderCreatedEvent.FromDomain(),
            OrderCancelledEvent orderCancelledEvent => orderCancelledEvent.FromDomain(),
            _ => throw new ArgumentOutOfRangeException(nameof(@event))
        };

    public (Infrastructure.Persistence.Orders.Order, IEnumerable<OrderEvent>) FromDomain(OrderAggregate aggregate, IEnumerable<IEvent> events)
    {
        throw new NotImplementedException();
    }

    public (Infrastructure.Persistence.Orders.Order, IEnumerable<OrderEvent>) FromDomain(OrderAggregate aggregate,
        IEnumerable<IOrderEvent> events)
    {
        var orderDto = FromDomain(aggregate);
        var eventsDto = events.Select(FromDomain).ToList();

        return (orderDto, eventsDto);
    }

    public override ErrorOr<OrderAggregate> ToDomain(Infrastructure.Persistence.Orders.Order dto)
    {
        if (!Guid.TryParse(dto.CustomerId, out Guid customerIdGuid))
        {
            return Error.Validation($"Invalid {dto.CustomerId}");
        }

        if (!Guid.TryParse(dto.Id, out Guid orderIdGuid))
        {
            return Error.Validation($"Invalid {dto.Id}");
        }

        if (!Guid.TryParse(dto.Metadata.StreamId, out Guid streamIdGuid))
        {
            return Error.Validation($"Invalid {dto.Metadata.StreamId}");
        }

        CustomerId customerId = new CustomerId(customerIdGuid);
        OrderId orderId = new OrderId(orderIdGuid);
        StreamId streamId = new StreamId(streamIdGuid);
        Domain.Core.Version version = new Domain.Core.Version(dto.Metadata.Version);

        return new OrderAggregate(dto.CreatedOnUtc, customerId)
        {
            CancelledOnUtc = dto.CancelledOnUtc,
            CompletedOnUtc = dto.CompletedOnUtc,
            Id = orderId,
            MetaData = new MetaData(streamId, version, dto.Metadata.Timestamp)
        };
    }

    public override ErrorOr<IOrderEvent> ToDomain(OrderEvent dto)
        => dto switch
        {
            Infrastructure.Persistence.Orders.OrderCancelledEvent orderCancelledEvent => orderCancelledEvent.ToDomain(),
            Infrastructure.Persistence.Orders.OrderCompletedEvent orderCompletedEvent => orderCompletedEvent.ToDomain(),
            Infrastructure.Persistence.Orders.OrderCreatedEvent orderCreatedEvent => orderCreatedEvent.ToDomain(),
            _ => throw new ArgumentOutOfRangeException(nameof(dto))
        };
}