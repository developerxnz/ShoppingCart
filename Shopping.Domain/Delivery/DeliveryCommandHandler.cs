using ErrorOr;
using Shopping.Domain.Core;
using Shopping.Domain.Delivery.Commands;
using Shopping.Domain.Delivery.Events;
using Shopping.Domain.Domain.Core.Handlers;
using Shopping.Domain.Extensions;
using Core_Version = Shopping.Domain.Core.Version;
using Version = Shopping.Domain.Core.Version;

namespace Shopping.Domain.Delivery.Core;

public interface IDeliveryCommandHandler
{
    ErrorOr<CommandResult<DeliveryAggregate, DeliveryEvent>> HandlerForNew(IDeliveryCommand command);

    ErrorOr<CommandResult<DeliveryAggregate, DeliveryEvent>> HandlerForExisting(IDeliveryCommand command, DeliveryAggregate aggregate);
}

public sealed class DeliveryCommandHandler : Handler<DeliveryAggregate, DeliveryEvent, IDeliveryCommand>, IDeliveryCommandHandler
{
    public override ErrorOr<CommandResult<DeliveryAggregate, DeliveryEvent>> HandlerForNew(IDeliveryCommand command)
    {
        switch (command)
        {
            case CreateDeliveryCommand createDeliveryCommand:
                return GenerateEventsForCreateDelivery(createDeliveryCommand);
            default:
                throw new ArgumentOutOfRangeException(nameof(command));
        }
    }

    private ErrorOr<CommandResult<DeliveryAggregate, DeliveryEvent>> GenerateEventsForCreateDelivery(CreateDeliveryCommand command)
    {
        DeliveryAggregate aggregate = new(
            command.CreatedOnUtc,
            command.OrderId
        );

        DeliveryCreatedEvent[] events =
        {
            new(
                command.CreatedOnUtc,
                command.CustomerId,
                command.OrderId,
                new Core_Version(1),
                command.CorrelationId,
                new CausationId(command.Id.Value))
        };

        return ApplyEvents(aggregate, events);
    }

    protected override ErrorOr<bool> AggregateCheck(IDeliveryCommand command, DeliveryAggregate aggregate)
    {
        DeliveryId deliveryId =
            (command switch
            {
                CompleteDeliveryCommand completeDeliveryCommand => completeDeliveryCommand.DeliveryId,
                CancelDeliveryCommand cancelDeliveryCommand => cancelDeliveryCommand.DeliveryId,
                _ => throw new ArgumentOutOfRangeException(nameof(command))
            });

        if (aggregate.Id != deliveryId)
        {
            return Error.Validation(Constants.InvalidAggregateForIdCode, Constants.InvalidAggregateForIdDescription);
        }

        return true;
    }

    protected override ErrorOr<CommandResult<DeliveryAggregate, DeliveryEvent>> ExecuteCommand(IDeliveryCommand command,
        DeliveryAggregate aggregate) =>
        (command switch
        {
            CompleteDeliveryCommand completeDeliveryCommand => GenerateEventsForDeliveryCompleted(
                completeDeliveryCommand, aggregate),
            _ => throw new ArgumentOutOfRangeException(nameof(command))
        })
        .Match(
            commandResult => ApplyEvents(commandResult.Aggregate, commandResult.Events),
            error => ErrorOr.ErrorOr.From(error).Value
        );

    private ErrorOr<CommandResult<DeliveryAggregate, DeliveryEvent>> GenerateEventsForDeliveryCompleted(
        CompleteDeliveryCommand command, DeliveryAggregate aggregate)
    {
        if (aggregate.DeliveredOnUtc.HasValue)
        {
            return Error.Validation(Constants.DeliveryAlreadyDeliveredCode,
                Constants.DeliveryAlreadyDeliveredDescription);
        }

        if (aggregate.MetaData.Version.Value == 0)
        {
            return Error.Validation(Constants.InvalidVersionCode, Constants.InvalidVersionDescription);
        }

        return new CommandResult<DeliveryAggregate, DeliveryEvent>(aggregate,
            new[]
            {
                new DeliveryCompletedEvent(command.CompletedOnUtc, command.CustomerId, command.DeliveryId,
                    command.OrderId,
                    aggregate.MetaData.Version.Increment(),
                    command.CorrelationId, new(command.Id.Value))
            });
    }

    protected override DeliveryAggregate Apply(DeliveryAggregate aggregate, DeliveryEvent @event)
    {
        MetaData metaData = aggregate.MetaData with {Version = @event.Version, TimeStamp = @event.TimeStamp};
        return @event switch
        {
            DeliveryCompletedEvent x => aggregate with {DeliveredOnUtc = x.CompletedOnUtc, MetaData = metaData},
            DeliveryCancelledEvent x => aggregate with {CancelledOnUtc = x.CancelledOnUtc, MetaData = metaData},
            _ => throw new ArgumentOutOfRangeException(nameof(@event))
        };
    }
}