using Shopping.Core;
using Shopping.Orders;
using Shopping.Orders.Core;
using Version = Shopping.Core.Version;

namespace Shopping;

public interface IOrder
{
    /// <summary>
    /// Creates a new Order
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="correlationId"></param>
    /// <returns></returns>
    Task CreateOrder(CustomerId customerId, CorrelationId correlationId);

    /// <summary>
    /// Completes and Existing Order
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="orderId"></param>
    /// <param name="correlationId"></param>
    /// <returns></returns>
    Task CompleteOrder(CustomerId customerId, OrderId orderId, CorrelationId correlationId);

    /// <summary>
    /// Cancels an Existing Order
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="orderId"></param>
    /// <param name="correlationId"></param>
    /// <returns></returns>
    Task CancelOrder(CustomerId customerId, OrderId orderId, CorrelationId correlationId);
}

public sealed class OrdersService : IOrder
{
    private readonly IOrderCommandHandler _commandHandler;

    public OrdersService(IOrderCommandHandler orderCommandHandler)
    {
        _commandHandler = orderCommandHandler;
    }

    public Task CreateOrder(CustomerId customerId, CorrelationId correlationId)
    {
        var command = new CreateOrderCommand(DateTime.UtcNow, customerId, correlationId);

        var commandResult = _commandHandler.HandlerForNew(command);

        return Task.CompletedTask;
    }

    public Task CompleteOrder(CustomerId customerId, OrderId orderId, CorrelationId correlationId)
    {
        //get aggregate from db here or outside
        Version version = new Version(2);
        var command = new CompleteOrderCommand(DateTime.UtcNow, customerId, orderId, correlationId);

        var aggregate = new OrderAggregate(DateTime.UtcNow,  customerId);
        
        var commandResult = _commandHandler.HandlerForExisting(command, aggregate);
        
        return Task.CompletedTask;
    }

    public Task CancelOrder(CustomerId customerId, OrderId orderId, CorrelationId correlationId)
    {
        IOrderCommand command = new CancelOrderCommand(DateTime.UtcNow, customerId, orderId, correlationId);
        DateTime aggregateDateTime = DateTime.UtcNow;

        //get order from db
        var aggregate = new OrderAggregate(
            aggregateDateTime, 
            customerId
        ) { MetaData = new MetaData(
            new StreamId(orderId.Value), 
            new Version(3), 
            aggregateDateTime
        )};
        
        var commandResult = _commandHandler.HandlerForExisting(command, aggregate);
        
        return Task.CompletedTask;
    }
}