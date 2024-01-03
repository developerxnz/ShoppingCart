using Shopping.Domain.Core;
using Shopping.Domain.Orders.Commands;
using Shopping.Domain.Orders.Core;
using Version = Shopping.Domain.Core.Version;

namespace Shopping.Domain.Orders.Events;

public abstract record OrderEvent(DateTime ModifiedDateUtc, Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, ModifiedDateUtc), IOrderEvent;

public record OrderCreatedEvent(DateTime CreatedOnUtc, CustomerId CustomerId, Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : OrderEvent(CreatedOnUtc, Version, CorrelationId, CausationId) { }

public record OrderCompletedEvent(DateTime CompletedOnUtc, CustomerId CustomerId, OrderId OrderId, Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : OrderEvent(CompletedOnUtc, Version, CorrelationId, CausationId) { }

public record OrderCancelledEvent(
        DateTime CancelledOnUtc, CustomerId CustomerId, OrderId OrderId, Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : OrderEvent(CancelledOnUtc, Version, CorrelationId, CausationId ) { }

public record CreateOrderData(DateTime CreatedOnUtc, CustomerId CustomerId);
public record CreateOrderCommand(DateTime CreatedOnUtc, CustomerId CustomerId, CorrelationId CorrelationId)
    : OrderCommand<CreateOrderData>(
        CorrelationId,
        new CreateOrderData(CreatedOnUtc, CustomerId)
    );


public interface IOrderEvent : IEvent { }