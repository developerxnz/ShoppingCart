using ErrorOr;
using Shopping.Domain.Core;
using Shopping.Domain.Delivery.Core;
using Shopping.Domain.Delivery.Requests;
using Shopping.Domain.Orders.Core;

namespace Shopping.Services.Delivery;

public interface IDeliveries
{
    /// <summary>
    /// Creates a new delivery for the given Order
    /// </summary>
    /// <returns></returns>
    Task<ErrorOr<DeliveryCreatedResponse>> CreateAsync(
        DateTime createdOnUtc, CustomerId customerId, OrderId orderId, CorrelationId correlationId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Cancels a delivery
    /// </summary>
    /// <returns></returns>
    Task<ErrorOr<DeliveryCancelledResponse>> CancelAsync(
        DateTime cancelledOnUtc, CustomerId customerId, OrderId orderId, DeliveryId deliveryId,
        CorrelationId correlationId, CancellationToken cancellationToken);

    /// <summary>
    /// Completes a delivery
    /// </summary>
    /// <returns></returns>
    Task<ErrorOr<DeliveryCompletedResponse>> CompleteAsync(
        DateTime completedOnUtc, CustomerId customerId, OrderId orderId, DeliveryId deliveryId,
        CorrelationId correlationId, CancellationToken cancellationToken);
}