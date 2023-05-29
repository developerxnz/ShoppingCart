using Shopping.Core;
using Shopping.Delivery.Core;
using Shopping.Domain.Core.Handlers;
using Shopping.Orders.Core;

namespace Shopping.Delivery.Commands;

public record DeliveryCommand<T>(CorrelationId CorrelationId, T Data) : Command<T>(CorrelationId, Data), IDeliveryCommand;

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
