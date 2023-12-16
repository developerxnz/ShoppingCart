using ErrorOr;
using Shopping.Domain.Core;
using Shopping.Domain.Domain.Core.Handlers;
using Shopping.Domain.Orders.Commands;
using Shopping.Domain.Orders.Core;
using Shopping.Domain.Orders.Events;
using Shopping.Domain.Extensions;
using Core_Version = Shopping.Domain.Core.Version;
using Version = Shopping.Domain.Core.Version;

namespace Shopping.Domain.Orders.Handlers;

public interface ICommandHandler
{
    ErrorOr<CommandResult<OrderAggregate>> HandlerForNew(IOrderCommand command);

    ErrorOr<CommandResult<OrderAggregate>> HandlerForExisting(IOrderCommand command, OrderAggregate aggregate);
}

public sealed class CommandHandler : Handler<OrderAggregate, IOrderCommand>, ICommandHandler
{
    public override ErrorOr<CommandResult<OrderAggregate>> HandlerForNew(IOrderCommand command)
    {
        switch (command)
        {
            case CreateOrderCommand createOrderCommand:
                return GenerateEventsForOrderCreated(createOrderCommand);
            default:
                return Error.Unexpected(Constants.InvalidCommandForNewCode,
                    string.Format(Constants.InvalidCommandForNewDescription, command.GetType()));
        }
    }

    protected override ErrorOr<bool> AggregateCheck(IOrderCommand command, OrderAggregate aggregate)
    {
        OrderId orderId =
            (command switch
            {
                CompleteOrderCommand addItemToCartCommand => addItemToCartCommand.OrderId,
                CancelOrderCommand removeItemFromCartCommand => removeItemFromCartCommand.OrderId,
                _ => throw new ArgumentOutOfRangeException(nameof(command))
            });

        if (aggregate.Id != orderId)
        {
            return Error.Validation(Constants.InvalidAggregateForIdCode, Constants.InvalidAggregateForIdDescription);
        }

        return true;
    }

    protected override ErrorOr<CommandResult<OrderAggregate>> ExecuteCommand(IOrderCommand command,
        OrderAggregate aggregate) =>
        (command switch
        {
            CompleteOrderCommand completeOrderCommand =>
                GenerateEventsForOrderCompleted(completeOrderCommand, aggregate),
            CancelOrderCommand cancelOrderCommand =>
                GenerateEventsForOrderCancelled(cancelOrderCommand, aggregate),
            _ => throw new ArgumentOutOfRangeException(nameof(command))
        })
        .Match(
            commandResult => ApplyEvents(commandResult.Aggregate, commandResult.Events),
            error => ErrorOr.ErrorOr.From(error).Value);

    private ErrorOr<CommandResult<OrderAggregate>> GenerateEventsForOrderCreated(CreateOrderCommand command)
    {
        OrderAggregate aggregate = new(command.CreatedOnUtc, command.CustomerId);
        OrderCreatedEvent[] events =
        {
            new(
                command.CreatedOnUtc,
                command.CustomerId,
                new Core_Version(1),
                command.CorrelationId,
                new CausationId(command.Id.Value))
        };
        
        return ApplyEvents(aggregate, events);
    }

    private ErrorOr<CommandResult<OrderAggregate>> GenerateEventsForOrderCompleted(CompleteOrderCommand command,
        OrderAggregate aggregate)
    {
        if (aggregate.CompletedOnUtc.HasValue)
        {
            return Error.Validation(Constants.OrderAlreadyCompletedCode, Constants.OrderAlreadyCompletedDescription);
        }

        if (aggregate.CancelledOnUtc.HasValue)
        {
            return Error.Validation(Constants.OrderCancelledCode, Constants.OrderCancelledDescription);
        }

        if (aggregate.MetaData.Version.Value == 0)
        {
            return Error.Validation(Constants.InvalidVersionCode, Constants.InvalidVersionDescription);
        }

        return new CommandResult<OrderAggregate>(aggregate,
            new[]
            {
                new OrderCompletedEvent(command.CompletedOnUtc, command.CustomerId, command.OrderId,
                    aggregate.MetaData.Version.Increment(),
                    command.CorrelationId, new CausationId(command.Id.Value))
            });
    }

    private ErrorOr<CommandResult<OrderAggregate>> GenerateEventsForOrderCancelled(CancelOrderCommand command,
        OrderAggregate aggregate)
    {
        if (aggregate.CancelledOnUtc.HasValue)
        {
            return Error.Validation(Constants.OrderCancelledCode, Constants.OrderCancelledDescription);
        }

        if (aggregate.CompletedOnUtc.HasValue)
        {
            return Error.Validation(Constants.OrderAlreadyCompletedCode, Constants.OrderAlreadyCompletedDescription);
        }

        if (aggregate.MetaData.Version.Value == 0)
        {
            return Error.Validation(Constants.InvalidVersionCode, Constants.InvalidVersionDescription);
        }

        return new CommandResult<OrderAggregate>(
            aggregate,
            new[]
            {
                new OrderCancelledEvent(command.CancelledOnUtc, command.CustomerId, command.OrderId,
                    aggregate.MetaData.Version.Increment(),
                    command.CorrelationId, new CausationId(command.Id.Value))
            });
    }

    protected override OrderAggregate Apply(OrderAggregate aggregate, IEvent @event)
    {
        MetaData metaData = aggregate.MetaData with {Version = @event.Version, TimeStamp = @event.TimeStamp};
        return @event switch
        {
            OrderCreatedEvent x => aggregate with {CreatedOnUtc = x.CreatedOnUtc, MetaData = metaData},
            OrderCancelledEvent x => aggregate with {CancelledOnUtc = x.CancelledOnUtc, MetaData = metaData},
            OrderCompletedEvent x => aggregate with {CompletedOnUtc = x.CompletedOnUtc, MetaData = metaData},
            _ => throw new ArgumentOutOfRangeException(nameof(@event))
        };
    }
}