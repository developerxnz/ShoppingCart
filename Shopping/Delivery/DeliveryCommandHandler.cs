using Shopping.Core;
using ErrorOr;
using Shopping.Domain.Core.Handlers;
using Shopping.Extensions;

namespace Shopping.Delivery.Core;

public interface IDeliveryCommandHandler
{
    ErrorOr<CommandResult<DeliveryAggregate>> HandlerForNew(ICommand command);

    ErrorOr<CommandResult<DeliveryAggregate>> HandlerForExisting(ICommand command, DeliveryAggregate aggregate);
}

public class DeliveryCommandHandler : Handler<DeliveryAggregate>, IDeliveryCommandHandler
{
    public ErrorOr<CommandResult<DeliveryAggregate>> HandlerForNew(ICommand command)
    {
        switch (command)
        {
            case CreateDeliveryCommand createDeliveryCommand:
                return GenerateEventsForCreateDelivery(createDeliveryCommand);
            default:
                throw new ArgumentOutOfRangeException(nameof(command));
        }
    }

    private ErrorOr<CommandResult<DeliveryAggregate>> GenerateEventsForCreateDelivery(CreateDeliveryCommand command)
    {
        DeliveryAggregate aggregate = new(
            command.CreatedOnUtc,
            command.OrderId
        );

        return new CommandResult<DeliveryAggregate>(
            aggregate,
            new[] {new DeliveryCreatedEvent(command.CreatedOnUtc, command.OrderId, new (1), command.CorrelationId, new (command.Id.Value))}
        );
    }

    public ErrorOr<CommandResult<DeliveryAggregate>> HandlerForExisting(ICommand command, DeliveryAggregate aggregate)
    {
        switch (command)
        {
            case CompleteDeliveryCommand completeDeliveryCommand:
                return GenerateEventsForDeliveryCompleted(completeDeliveryCommand, aggregate)
                    .Match(
                        delivery => ApplyEvents(delivery.Aggregate, delivery.Events),
                        error => ErrorOr.ErrorOr.From(error).Value
                    );
            default:
                throw new ArgumentOutOfRangeException(nameof(command));
                
        }
    }
    
    private ErrorOr<CommandResult<DeliveryAggregate>> GenerateEventsForDeliveryCompleted(CompleteDeliveryCommand command, DeliveryAggregate aggregate)
    {
        if (aggregate.DeliveredOnUtc.HasValue)
        {
            return Error.Validation(Constants.DeliveryAlreadyDeliveredCode, Constants.DeliveryAlreadyDeliveredDescription);
        }

        if (aggregate.MetaData.Version.Value == 0)
        {
            return Error.Validation(Constants.InvalidVersionCode, Constants.InvalidVersionDescription);
        }

        return new CommandResult<DeliveryAggregate>(aggregate,
            new[]
            {
                new DeliveryCompletedEvent(command.CompletedOnUtc, command.DeliveryId, command.OrderId,
                    aggregate.MetaData.Version.Increment(),
                    command.CorrelationId, new(command.Id.Value))
            });
    }

    protected override DeliveryAggregate Apply(DeliveryAggregate aggregate,IEvent @event)
    {
        MetaData metaData = aggregate.MetaData with {Version = @event.Version,TimeStamp = @event.TimeStamp};
        return @event switch
        {
            DeliveryCompletedEvent x => aggregate with {DeliveredOnUtc = x.CompletedOnUtc, MetaData = metaData},
            _ => throw new ArgumentOutOfRangeException(nameof(@event))
        };
    }
}