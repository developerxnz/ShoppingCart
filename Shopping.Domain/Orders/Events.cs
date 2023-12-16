using Shopping.Domain.Core;
using Shopping.Domain.Orders.Commands;
using Shopping.Domain.Orders.Core;
using Version = Shopping.Domain.Core.Version;

namespace Shopping.Domain.Orders.Events;

public record OrderCreatedEvent(DateTime CreatedOnUtc, CustomerId CustomerId, Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, CreatedOnUtc) { }

public record OrderCompletedEvent(DateTime CompletedOnUtc, CustomerId CustomerId, OrderId OrderId, Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, CompletedOnUtc) { }

public record OrderCancelledEvent(
        DateTime CancelledOnUtc, CustomerId CustomerId, OrderId OrderId, Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, CancelledOnUtc) { }

public record CreateOrderData(DateTime CreatedOnUtc, CustomerId CustomerId);
public record CreateOrderCommand(DateTime CreatedOnUtc, CustomerId CustomerId, CorrelationId CorrelationId)
    : OrderCommand<CreateOrderData>(
        CorrelationId,
        new CreateOrderData(CreatedOnUtc, CustomerId)
    );
