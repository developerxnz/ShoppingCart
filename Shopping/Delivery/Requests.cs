using Shopping.Core;
using Shopping.Delivery.Core;

namespace Shopping.Delivery.Requests;

public sealed record DeliveryCreatedResponse(DeliveryId DeliverId, CorrelationId CorrelationId);
public sealed record DeliveryCancelledResponse(DeliveryId DeliverId, CorrelationId CorrelationId);

public sealed record DeliveryCompletedResponse(DeliveryId DeliverId, CorrelationId CorrelationId);

public sealed record CreateDeliveryRequest();
public sealed record CreateDeliveryResponse();