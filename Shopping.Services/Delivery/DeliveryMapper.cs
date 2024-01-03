using ErrorOr;
using Shopping.Domain.Core;
using Shopping.Domain.Delivery.Core;
using Shopping.Domain.Delivery.Events;
using Shopping.Domain.Orders.Core;
using Shopping.Services.Interfaces;
using DeliveryEvent = Shopping.Infrastructure.Persistence.Delivery.DeliveryEvent;
using Metadata = Shopping.Domain.Core.Persistence.Metadata;
using Version = Shopping.Domain.Core.Version;
using Shopping.Infrastructure.Persistence.Delivery;

namespace Shopping.Services.Delivery;

public class DeliveryMapper :
    Mapper<Domain.Delivery.Core.DeliveryAggregate, Infrastructure.Persistence.Delivery.Delivery, IDeliveryEvent, Infrastructure.Persistence.Delivery.DeliveryEvent>,
    IMapper<Domain.Delivery.Core.DeliveryAggregate, Infrastructure.Persistence.Delivery.Delivery, IDeliveryEvent, Infrastructure.Persistence.Delivery.DeliveryEvent>
{
    public override Infrastructure.Persistence.Delivery.Delivery FromDomain(DeliveryAggregate aggregate)
    {
        return new Infrastructure.Persistence.Delivery.Delivery
        {
            Id = aggregate.Id.Value.ToString(),
            CreatedOnUtc = aggregate.CreatedOnUtc,
            DeliveredOnUtc = aggregate.DeliveredOnUtc,
            Metadata = new Metadata(
                aggregate.MetaData.StreamId.Value.ToString(),
                aggregate.MetaData.Version.Value,
                aggregate.MetaData.TimeStamp),
            OrderId = aggregate.OrderId.Value.ToString()
        };
    }

    public override DeliveryEvent FromDomain(IDeliveryEvent @event)
        => @event switch
        {
            Domain.Delivery.Events.DeliveryCompletedEvent deliveryCompletedEvent => deliveryCompletedEvent.FromDomain(),
            Domain.Delivery.Events.DeliveryCreatedEvent deliveryCreatedEvent => deliveryCreatedEvent.FromDomain(),
            Domain.Delivery.Events.DeliveryCancelledEvent deliveryCancelledEvent => deliveryCancelledEvent.FromDomain(),
            _ => throw new ArgumentOutOfRangeException(nameof(@event))
        };

    public (Infrastructure.Persistence.Delivery.Delivery, IEnumerable<DeliveryEvent>) FromDomain(DeliveryAggregate aggregate, IEnumerable<IDeliveryEvent> events)
    {
        var aggregateDto = FromDomain(aggregate);
        var eventDtos = events.Select(FromDomain).ToList();

        return new(aggregateDto, eventDtos);
    }

    public override ErrorOr<DeliveryAggregate> ToDomain(Infrastructure.Persistence.Delivery.Delivery dto)
    {
        if (!Guid.TryParse(dto.OrderId, out Guid orderIdGuid))
        {
            return Error.Validation($"Invalid {nameof(dto.OrderId)}");
        }

        if (!Guid.TryParse(dto.Id, out Guid id))
        {
            return Error.Validation($"Invalid {nameof(dto.Id)}");
        }

        if (!Guid.TryParse(dto.Metadata.StreamId, out Guid streamIdGuid))
        {
            return Error.Validation($"Invalid {nameof(dto.Metadata.StreamId)}");
        }

        DeliveryId deliveryId = new(id);
        OrderId orderId = new(orderIdGuid);
        StreamId streamId = new(streamIdGuid);
        Version version = new Version(dto.Metadata.Version);

        return new DeliveryAggregate(dto.CreatedOnUtc, orderId)
        {
            Id = deliveryId,
            DeliveredOnUtc = dto.DeliveredOnUtc,
            MetaData = new Domain.Core.MetaData(streamId, version, dto.Metadata.Timestamp)
        };
    }

    public override ErrorOr<IDeliveryEvent> ToDomain(DeliveryEvent dto)
        => dto switch
        {
            Infrastructure.Persistence.Delivery.DeliveryCreatedEvent deliveryCreatedEvent => deliveryCreatedEvent.ToDomain(),
            Infrastructure.Persistence.Delivery.DeliveryCompletedEvent deliveryCompletedEvent => deliveryCompletedEvent.ToDomain(),
            Infrastructure.Persistence.Delivery.DeliveryCancelledEvent deliveryCancelledEvent => deliveryCancelledEvent.ToDomain(),
            _ => throw new ArgumentOutOfRangeException(nameof(dto))
        };
}