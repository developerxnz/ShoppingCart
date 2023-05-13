using Shopping.Core;
using Shopping.Delivery.Core;
using Shopping.Orders.Core;

namespace Shopping.Delivery.Events;

    
public record DeliveryCreatedEvent(
        DateTime CreatedOnUtc, CustomerId CustomerId, OrderId OrderId, Shopping.Core.Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, CreatedOnUtc) { }
    
public record DeliveryCompletedEvent(
        DateTime CompletedOnUtc, CustomerId CustomerId, DeliveryId DeliveryId, OrderId OrderId, Shopping.Core.Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, CompletedOnUtc) { }
    
public record DeliveryCancelledEvent(
        DateTime CancelledOnUtc, CustomerId CustomerId, DeliveryId DeliveryId, OrderId OrderId, Shopping.Core.Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, CancelledOnUtc) { }