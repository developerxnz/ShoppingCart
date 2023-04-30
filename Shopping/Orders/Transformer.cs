using ErrorOr;
using Shopping.Core;
using Shopping.Orders.Core;
using Shopping.Orders.Persistence;

namespace Shopping.Orders;

public class Transformer : Transformer<OrderAggregate, Persistence.Order>
{
    public override Order FromDomain(OrderAggregate domain)
    {
        return new Order(
            domain.Id.ToString(),
            domain.CancelledOnUtc,
            domain.CompletedOnUtc,
            domain.CreatedOnUtc,
            domain.CustomerId.Value.ToString(),
            new Shopping.Core.Persistence.MetaData(
                domain.MetaData.StreamId.Value.ToString(),
                domain.MetaData.Version.Value,
                domain.MetaData.TimeStamp)
        );
    }

    public override ErrorOr<OrderAggregate> ToDomain(Order dto)
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
        Shopping.Core.Version version = new Shopping.Core.Version(dto.MetaData.Version);

        return new OrderAggregate(dto.CreatedOnUtc, customerId )
        {
            Id = orderId,
            CancelledOnUtc = dto.CancelledOnUtc,
            CompletedOnUtc = dto.CompletedOnUtc,
            CreatedOnUtc = dto.CreatedOnUtc,
            MetaData = new MetaData(streamId, version, dto.MetaData.TimeStamp)
        };
    }
}