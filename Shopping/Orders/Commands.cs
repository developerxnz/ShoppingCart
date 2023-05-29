using Shopping.Core;
using Shopping.Domain.Core.Handlers;
using Shopping.Orders.Core;

namespace Shopping.Orders.Commands;

public interface IOrderCommand : ICommand {}

public record OrderCommand<T>(CorrelationId CorrelationId, T Data) : Command<T>(CorrelationId, Data), IOrderCommand;

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