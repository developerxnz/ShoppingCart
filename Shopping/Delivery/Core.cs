using Shopping.Core;
using Shopping.Domain.Core.Handlers;
using Shopping.Orders.Core;

namespace Shopping.Delivery.Core;

public record DeliveryId (Guid Value);

public record Delivery(DeliveryId DeliveryId, DateTime CreatedOnUtc, DateTime DeliveredOnUtc);

public record CreateDeliveryData(DateTime CreatedOnUtc, OrderId OrderId);
public record CreateDeliveryCommand(DateTime CreatedOnUtc, OrderId OrderId, CorrelationId CorrelationId ) 
    : Command<CreateDeliveryData>(
        CorrelationId, 
        new CreateDeliveryData(CreatedOnUtc, OrderId)
    );

public record CompleteDeliveryData(DateTime CompletedOnUtc, OrderId OrderId, DeliveryId DeliverId);
public record CompleteDeliveryCommand(DateTime CompletedOnUtc, DeliveryId DeliveryId, OrderId OrderId, CorrelationId CorrelationId ) 
    : Command<CompleteDeliveryData>(
        CorrelationId, 
        new CompleteDeliveryData(CompletedOnUtc, OrderId, DeliveryId)
    );
    
public record DeliveryCreatedEvent(
        DateTime CreatedOnUtc, OrderId OrderId, Shopping.Core.Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, CreatedOnUtc) { }
    
public record DeliveryCompletedEvent(
        DateTime CompletedOnUtc, DeliveryId DeliveryId, OrderId OrderId, Shopping.Core.Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, CompletedOnUtc) { }