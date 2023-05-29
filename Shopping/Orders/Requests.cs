using Shopping.Core;
using Shopping.Orders.Core;

namespace Shopping.Orders.Requests;

public record CancelOrderResponse(OrderId OrderId, CorrelationId CorrelationId);

public record CompleteOrderResponse(OrderId OrderId, CorrelationId CorrelationId);

public record CreateOrderResponse(OrderId OrderId, CorrelationId CorrelationId);