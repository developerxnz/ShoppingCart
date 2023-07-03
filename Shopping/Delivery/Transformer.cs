using ErrorOr;
using Shopping.Core;
using Shopping.Delivery.Core;
using Shopping.Orders.Core;
using Metadata = Shopping.Core.Persistence.Metadata;
using Version = Shopping.Core.Version;

namespace Shopping.Delivery;

public class Transformer : Transformer<DeliveryAggregate, Persistence.Delivery>
{
    public override Persistence.Delivery FromDomain(DeliveryAggregate aggregate)
    {
        return new Persistence.Delivery(
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

    public override ErrorOr<DeliveryAggregate> ToDomain(Persistence.Delivery dto)
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
            MetaData = new Shopping.Core.MetaData(streamId, version, dto.Metadata.Timestamp)
        };
    }
}