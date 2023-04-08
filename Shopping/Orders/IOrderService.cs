using Shopping.Core;
using Shopping.Orders.Core;

namespace Shopping;

public interface IOrderService
{
    Task CreateOrder(CustomerId customerId, CorrelationId correlationId);

    Task CompleteOrder(CustomerId customerId, OrderId orderId, CorrelationId correlationId);

    Task CancelOrder(CustomerId customerId, OrderId orderId, CorrelationId correlationId);
}