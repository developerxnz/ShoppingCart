using Shopping.Domain.Core;
using Shopping.Domain.Delivery.Core;
using Shopping.Domain.Orders.Core;
using Version = Shopping.Domain.Core.Version;

namespace Shopping.Domain.Delivery.Events;

public abstract record DeliveryEvent(DateTime ModifiedDateUtc, Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, ModifiedDateUtc), IDeliveryEvent;    

public record DeliveryCreatedEvent(
        DateTime CreatedOnUtc, CustomerId CustomerId, OrderId OrderId, Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : DeliveryEvent(CreatedOnUtc, Version, CorrelationId, CausationId ) { }
    
public record DeliveryCompletedEvent(
        DateTime CompletedOnUtc, CustomerId CustomerId, DeliveryId DeliveryId, OrderId OrderId, Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : DeliveryEvent(CompletedOnUtc,Version, CorrelationId, CausationId) { }
    
public record DeliveryCancelledEvent(
        DateTime CancelledOnUtc, CustomerId CustomerId, DeliveryId DeliveryId, OrderId OrderId, Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : DeliveryEvent(CancelledOnUtc, Version, CorrelationId, CausationId) { }
    
public interface IDeliveryEvent : IEvent { }