using Shopping.Core;
using Shopping.Orders;
using Shopping.Orders.Core;
using Version = Shopping.Core.Version;

namespace Shopping;

public class OrdersService : IOrderService
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
            new(orderId.Value), 
            new(3), 
            aggregateDateTime
        )};
        
        var commandResult = _commandHandler.HandlerForExisting(command, aggregate);
        
        return Task.CompletedTask;
    }
}