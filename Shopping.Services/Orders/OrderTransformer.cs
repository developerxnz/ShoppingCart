using ErrorOr;
using Shopping.Domain.Core;
using Shopping.Domain.Orders;
using Shopping.Domain.Orders.Core;

namespace Shopping.Services.Orders;

public class OrderTransformer : Transformer<OrderAggregate, Infrastructure.Persistence.Orders.Order>
{
    public override Infrastructure.Persistence.Orders.Order FromDomain(OrderAggregate aggregate)
    {
        return new Infrastructure.Persistence.Orders.Order(
            aggregate.Id.ToString(),
            aggregate.CancelledOnUtc,
            aggregate.CompletedOnUtc,
            aggregate.CreatedOnUtc,
            aggregate.CustomerId.Value.ToString(),
            new Domain.Core.Persistence.Metadata(
                aggregate.MetaData.StreamId.Value.ToString(),
                aggregate.MetaData.Version.Value,
                aggregate.MetaData.TimeStamp)
        );
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
        
        if (!Guid.TryParse(dto.MetaData.StreamId, out Guid streamIdGuid))
        {
            return Error.Validation($"Invalid {dto.MetaData.StreamId}");
        }

        CustomerId customerId = new CustomerId(customerIdGuid);
        OrderId orderId = new OrderId(orderIdGuid);
        StreamId streamId = new StreamId(streamIdGuid);
        Domain.Core.Version version = new Domain.Core.Version(dto.MetaData.Version);

        return new OrderAggregate(dto.CreatedOnUtc, customerId )
        {
            Id = orderId,
            CancelledOnUtc = dto.CancelledOnUtc,
            CompletedOnUtc = dto.CompletedOnUtc,
            CreatedOnUtc = dto.CreatedOnUtc,
            MetaData = new MetaData(streamId, version, dto.MetaData.Timestamp)
        };
    }
}