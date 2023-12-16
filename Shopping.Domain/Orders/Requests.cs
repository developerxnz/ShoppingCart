using Shopping.Domain.Core;
using Shopping.Domain.Orders.Core;

namespace Shopping.Domain.Orders.Requests;

public record CancelOrderResponse(OrderId OrderId, CorrelationId CorrelationId);

public record CompleteOrderResponse(OrderId OrderId, CorrelationId CorrelationId);

public record CreateOrderResponse(OrderId OrderId, CorrelationId CorrelationId);