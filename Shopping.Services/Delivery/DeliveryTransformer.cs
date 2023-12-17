using ErrorOr;
using Shopping.Domain.Core;
using Shopping.Domain.Delivery.Core;
using Shopping.Domain.Orders.Core;
using Shopping.Services.Interfaces;
using Metadata = Shopping.Domain.Core.Persistence.Metadata;
using Version = Shopping.Domain.Core.Version;

namespace Shopping.Services.Delivery;

public class DeliveryTransformer : Transformer<DeliveryAggregate, Infrastructure.Persistence.Delivery.Delivery>, ITransformer<DeliveryAggregate, Infrastructure.Persistence.Delivery.Delivery>
{
    public override Infrastructure.Persistence.Delivery.Delivery FromDomain(DeliveryAggregate aggregate)
    {
        return new Infrastructure.Persistence.Delivery.Delivery(
            aggregate.Id.Value.ToString(),
            aggregate.CreatedOnUtc,
            aggregate.DeliveredOnUtc,
            new Metadata(
                aggregate.MetaData.StreamId.Value.ToString(),
                aggregate.MetaData.Version.Value,
                aggregate.MetaData.TimeStamp),
            aggregate.OrderId.Value.ToString()
        );
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
}