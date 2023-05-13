using ErrorOr;
using Shopping.Core;
using Shopping.Orders;
using Shopping.Orders.Core;

namespace Shopping.Delivery.Services;

public interface IOrder
{
    /// <summary>
    /// Creates a new Order
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="correlationId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<CreateOrderResponse>> CreateOrder(CustomerId customerId, CorrelationId correlationId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Completes and Existing Order
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="orderId"></param>
    /// <param name="correlationId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<CompleteOrderResponse>> CompleteOrder(CustomerId customerId, OrderId orderId,
        CorrelationId correlationId, CancellationToken cancellationToken);

    /// <summary>
    /// Cancels an Existing Order
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="orderId"></param>
    /// <param name="correlationId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<CancelOrderResponse>> CancelOrder(CustomerId customerId, OrderId orderId,
        CorrelationId correlationId, CancellationToken cancellationToken);
}

public sealed class Order : Service<OrderAggregate, Shopping.Orders.Persistence.Order>, IOrder
{
    private readonly IOrderCommandHandler _commandHandler;
    private readonly ITransformer<OrderAggregate, Shopping.Orders.Persistence.Order> _transformer;

    public Order(IOrderCommandHandler orderCommandHandler, IRepository<Shopping.Orders.Persistence.Order> repository,
        ITransformer<OrderAggregate, Orders.Persistence.Order> transformer) :
        base(repository)
    {
        _commandHandler = orderCommandHandler;
        _transformer = transformer;
    }

    public async Task<ErrorOr<CreateOrderResponse>> CreateOrder(CustomerId customerId, CorrelationId correlationId,
        CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(DateTime.UtcNow, customerId, correlationId);

        var commandResult = _commandHandler.HandlerForNew(command);
        if (commandResult.IsError)
        {
            return ErrorOr.ErrorOr.From(commandResult.Errors).Value;
        }

        await SaveAsync(commandResult.Value.Aggregate, commandResult.Value.Events, cancellationToken);

        return new CreateOrderResponse(commandResult.Value.Aggregate.Id, correlationId);
    }

    public async Task<ErrorOr<CompleteOrderResponse>> CompleteOrder(CustomerId customerId, OrderId orderId,
        CorrelationId correlationId, CancellationToken cancellationToken)
    {
        PartitionKey partitionKey = new PartitionKey(customerId.Value.ToString());
        Id id = new Id(orderId.Value.ToString());
        
        var aggregateResult = await LoadAsync(partitionKey, id, cancellationToken);
        if (aggregateResult.IsError)
        {
            return ErrorOr.ErrorOr.From(aggregateResult.Errors).Value;
        }

        var command = new CompleteOrderCommand(DateTime.UtcNow, customerId, orderId, correlationId);
        var commandResult = _commandHandler.HandlerForExisting(command, aggregateResult.Value);
        if (commandResult.IsError)
        {
            return ErrorOr.ErrorOr.From(commandResult.Errors).Value;
        }

        await SaveAsync(commandResult.Value.Aggregate, commandResult.Value.Events, cancellationToken);

        return new CompleteOrderResponse(commandResult.Value.Aggregate.Id, correlationId);
    }

    public async Task<ErrorOr<CancelOrderResponse>> CancelOrder(CustomerId customerId, OrderId orderId, CorrelationId correlationId,
        CancellationToken cancellationToken)
    {
        PartitionKey partitionKey = new PartitionKey(customerId.Value.ToString());
        Id id = new Id(orderId.Value.ToString());
        
        var aggregateResult = await LoadAsync(partitionKey, id, cancellationToken);
        if (aggregateResult.IsError)
        {
            return ErrorOr.ErrorOr.From(aggregateResult.Errors).Value;
        }
        
        IOrderCommand command = new CancelOrderCommand(DateTime.UtcNow, customerId, orderId, correlationId);
        var commandResult = _commandHandler.HandlerForExisting(command, aggregateResult.Value);
        if (commandResult.IsError)
        {
            return ErrorOr.ErrorOr.From(commandResult.Errors).Value;
        }
        
        await SaveAsync(commandResult.Value.Aggregate, commandResult.Value.Events, cancellationToken);

        return new CancelOrderResponse(commandResult.Value.Aggregate.Id, correlationId);
    }

    protected override ErrorOr<OrderAggregate> ToDomain(Orders.Persistence.Order persistenceAggregate)
    {
        return _transformer.ToDomain(persistenceAggregate);
    }

    protected override Orders.Persistence.Order FromDomain(OrderAggregate aggregate)
    {
        return _transformer.FromDomain(aggregate);
    }
}