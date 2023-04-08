using Shopping.Core;
using Shopping.Domain.Core.Handlers;
using Version = Shopping.Core.Version;

namespace Shopping.Orders.Core;

public interface IOrderCommand : ICommand {}

public record OrderCommand<T>(CorrelationId CorrelationId, T Data) : Command<T>(CorrelationId, Data), IOrderCommand;


public record OrderId(Guid Value);

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

public record CompleteOrderData(DateTime CompletedOnUtc, CustomerId CustomerId, OrderId OrderId);
public record CompleteOrderCommand(DateTime CompletedOnUtc, CustomerId CustomerId, OrderId OrderId, CorrelationId CorrelationId)
    : OrderCommand<CompleteOrderData>(
        CorrelationId,
        new CompleteOrderData(CompletedOnUtc, CustomerId, OrderId)
    );

public record CancelOrderData(DateTime CancelledOnUtc, CustomerId CustomerId, OrderId OrderId);
public record CancelOrderCommand(DateTime CancelledOnUtc, CustomerId CustomerId, OrderId OrderId, CorrelationId CorrelationId ) 
    : OrderCommand<CancelOrderData>(
        CorrelationId, 
        new CancelOrderData(CancelledOnUtc, CustomerId, OrderId)
    );