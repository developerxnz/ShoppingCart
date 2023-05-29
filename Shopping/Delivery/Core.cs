using Shopping.Domain.Core.Handlers;

namespace Shopping.Delivery.Core;

public interface IDeliveryCommand : ICommand {}

public record DeliveryId(Guid Value)
{
    public static DeliveryId Create()
    {
        return new DeliveryId(Guid.NewGuid());
    }
};
