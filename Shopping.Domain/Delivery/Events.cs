using Shopping.Domain.Core;
using Shopping.Domain.Delivery.Core;
using Shopping.Domain.Orders.Core;
using Version = Shopping.Domain.Core.Version;

namespace Shopping.Domain.Delivery.Events;

    
public record DeliveryCreatedEvent(
        DateTime CreatedOnUtc, CustomerId CustomerId, OrderId OrderId, Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, CreatedOnUtc) { }
    
public record DeliveryCompletedEvent(
        DateTime CompletedOnUtc, CustomerId CustomerId, DeliveryId DeliveryId, OrderId OrderId, Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, CompletedOnUtc) { }
    
public record DeliveryCancelledEvent(
        DateTime CancelledOnUtc, CustomerId CustomerId, DeliveryId DeliveryId, OrderId OrderId, Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, CancelledOnUtc) { }