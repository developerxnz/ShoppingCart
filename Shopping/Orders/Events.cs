using Shopping.Core;
using Shopping.Orders.Commands;
using Shopping.Orders.Core;

namespace Shopping.Orders.Events;

public record OrderCreatedEvent(DateTime CreatedOnUtc, CustomerId CustomerId, Shopping.Core.Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, CreatedOnUtc) { }

public record OrderCompletedEvent(DateTime CompletedOnUtc, CustomerId CustomerId, OrderId OrderId, Shopping.Core.Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, CompletedOnUtc) { }

public record OrderCancelledEvent(
        DateTime CancelledOnUtc, CustomerId CustomerId, OrderId OrderId, Shopping.Core.Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, CancelledOnUtc) { }

public record CreateOrderData(DateTime CreatedOnUtc, CustomerId CustomerId);
public record CreateOrderCommand(DateTime CreatedOnUtc, CustomerId CustomerId, CorrelationId CorrelationId)
    : OrderCommand<CreateOrderData>(
        CorrelationId,
        new CreateOrderData(CreatedOnUtc, CustomerId)
    );
