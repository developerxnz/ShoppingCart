using ErrorOr;
using Shopping.Domain.Core;
using Shopping.Domain.Orders.Core;
using Shopping.Domain.Orders.Requests;

namespace Shopping.Services.Orders;

public interface IOrder
{
    /// <summary>
    /// Creates a new Order
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="correlationId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<CreateOrderResponse>> CreateOrder(CustomerId customerId, CorrelationId correlationId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Completes and Existing Order
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="orderId"></param>
    /// <param name="correlationId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<CompleteOrderResponse>> CompleteOrder(CustomerId customerId, OrderId orderId,
        CorrelationId correlationId, CancellationToken cancellationToken);

    /// <summary>
    /// Cancels an Existing Order
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="orderId"></param>
    /// <param name="correlationId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<CancelOrderResponse>> CancelOrder(CustomerId customerId, OrderId orderId,
        CorrelationId correlationId, CancellationToken cancellationToken);
}