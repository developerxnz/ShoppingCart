using Shopping.Core;
using Shopping.Domain.Core.Handlers;
using Shopping.Orders.Core;

namespace Shopping.Delivery.Core;

public interface IDeliveryCommand : ICommand {}

public record DeliveryCommand<T>(CorrelationId CorrelationId, T Data) : Command<T>(CorrelationId, Data), IDeliveryCommand;

public record DeliveryId (Guid Value);

public record CreateDeliveryData(DateTime CreatedOnUtc, CustomerId CustomerId, OrderId OrderId);
public record CreateDeliveryCommand(DateTime CreatedOnUtc, CustomerId CustomerId, OrderId OrderId, CorrelationId CorrelationId ) 
    : DeliveryCommand<CreateDeliveryData>(
        CorrelationId, 
        new CreateDeliveryData(CreatedOnUtc, CustomerId, OrderId)
    );

public record CancelDeliveryData(DateTime CancelledOnUtc, CustomerId CustomerId, OrderId OrderId, DeliveryId DeliverId);
public record CancelDeliveryCommand(DateTime CancelledOnUtc, CustomerId CustomerId, OrderId OrderId, DeliveryId DeliveryId, CorrelationId CorrelationId ) 
    : DeliveryCommand<CancelDeliveryData>(
        CorrelationId, 
        new CancelDeliveryData(CancelledOnUtc, CustomerId, OrderId, DeliveryId)
    );

public record CompleteDeliveryData(DateTime CompletedOnUtc, CustomerId CustomerId, OrderId OrderId, DeliveryId DeliverId);
public record CompleteDeliveryCommand(DateTime CompletedOnUtc, CustomerId CustomerId, DeliveryId DeliveryId, OrderId OrderId, CorrelationId CorrelationId ) 
    : DeliveryCommand<CompleteDeliveryData>(
        CorrelationId, 
        new CompleteDeliveryData(CompletedOnUtc, CustomerId, OrderId, DeliveryId)
    );
    
public record DeliveryCreatedEvent(
        DateTime CreatedOnUtc, CustomerId CustomerId, OrderId OrderId, Shopping.Core.Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, CreatedOnUtc) { }
    
public record DeliveryCompletedEvent(
        DateTime CompletedOnUtc, CustomerId CustomerId, DeliveryId DeliveryId, OrderId OrderId, Shopping.Core.Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, CompletedOnUtc) { }
    
public record DeliveryCancelledEvent(
        DateTime CancelledOnUtc, CustomerId CustomerId, DeliveryId DeliveryId, OrderId OrderId, Shopping.Core.Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, CancelledOnUtc) { }

public sealed record DeliveryCreatedResponse(DeliveryId DeliverId, CorrelationId CorrelationId);
public sealed record DeliveryCancelledResponse(DeliveryId DeliverId, CorrelationId CorrelationId);

public sealed record DeliveryCompletedResponse(DeliveryId DeliverId, CorrelationId CorrelationId);

public sealed record CreateDeliveryRequest();
public sealed record CreateDeliveryResponse();