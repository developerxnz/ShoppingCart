using ErrorOr;
using Shopping.Core;
using Shopping.Delivery.Core;
using Shopping.Orders.Core;
using MetaData = Shopping.Core.Persistence.MetaData;
using Version = Shopping.Core.Version;

namespace Shopping.Delivery;

public class Transformer : Transformer<DeliveryAggregate, Persistence.Delivery>
{
    public override Persistence.Delivery FromDomain(DeliveryAggregate domain)
    {
        return new Persistence.Delivery(
            domain.Id.Value.ToString(),
            domain.CreatedOnUtc,
            domain.DeliveredOnUtc,
            new MetaData(
                domain.MetaData.StreamId.Value.ToString(),
                domain.MetaData.Version.Value,
                domain.MetaData.TimeStamp),
            domain.OrderId.Value.ToString()
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

        if (!Guid.TryParse(dto.MetaData.StreamId, out Guid streamIdGuid))
        {
            return Error.Validation($"Invalid {nameof(dto.MetaData.StreamId)}");
        }

        DeliveryId deliveryId = new(id);
        OrderId orderId = new(orderIdGuid);
        StreamId streamId = new(streamIdGuid);
        Version version = new Version(dto.MetaData.Version);

        return new DeliveryAggregate(dto.CreatedOnUtc, orderId)
        {
            Id = deliveryId,
            DeliveredOnUtc = dto.DeliveredOnUtc,
            MetaData = new Shopping.Core.MetaData(streamId, version, dto.MetaData.TimeStamp)
        };
    }
}