using Shopping.Core;
using Shopping.Delivery.Command;
using Shopping.Domain.Core.Handlers;
using Shopping.Orders.Core;

namespace Shopping.Delivery.Core;

public interface IDeliveryCommand : ICommand {}

public record DeliveryId(Guid Value)
{
    public static DeliveryId Create()
    {
        return new DeliveryId(Guid.NewGuid());
    }
};
