using ErrorOr;
using Shopping.Domain.Core;
using Shopping.Domain.Orders;
using Shopping.Domain.Orders.Commands;
using Shopping.Domain.Orders.Core;
using Shopping.Domain.Orders.Events;
using Shopping.Domain.Orders.Handlers;
using Shopping.Domain.Orders.Requests;
using Shopping.Infrastructure.Interfaces;
using Shopping.Services.Interfaces;
using IEvent = Shopping.Infrastructure.Interfaces.IEvent;

namespace Shopping.Services.Orders;

public sealed class OrderService : Service<OrderAggregate, Infrastructure.Persistence.Orders.Order, OrderEvent>, IOrder
{
    private readonly ICommandHandler _commandHandler;

    private readonly IMapper<OrderAggregate, Infrastructure.Persistence.Orders.Order, IOrderEvent,
        Infrastructure.Persistence.Orders.OrderEvent> _mapper;

    public OrderService(ICommandHandler orderCommandHandler, IRepository<Infrastructure.Persistence.Orders.Order> repository,
        IMapper<OrderAggregate, Infrastructure.Persistence.Orders.Order, IOrderEvent,
            Infrastructure.Persistence.Orders.OrderEvent> mapper) :
        base(repository)
    {
        _commandHandler = orderCommandHandler;
        _mapper = mapper;
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

    public async Task<ErrorOr<CancelOrderResponse>> CancelOrder(CustomerId customerId, OrderId orderId,
        CorrelationId correlationId,
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

    protected override ErrorOr<OrderAggregate> ToDomain(Infrastructure.Persistence.Orders.Order aggregate)
        => _mapper.ToDomain(aggregate);

    protected override (Infrastructure.Persistence.Orders.Order, IEnumerable<IEvent>) 
        FromDomain(OrderAggregate aggregate, IEnumerable<OrderEvent> events)
        => _mapper.FromDomain(aggregate, events);
}