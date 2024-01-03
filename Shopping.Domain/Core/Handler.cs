using ErrorOr;
using Shopping.Domain.Domain.Core.Handlers;

namespace Shopping.Domain.Core;

public abstract class Handler<TAggregate, TEvent, TCommand> 
    where TAggregate: IAggregate
    where TEvent: IEvent
{
    public abstract ErrorOr<CommandResult<TAggregate, TEvent>> HandlerForNew(TCommand command);
    
    public ErrorOr<CommandResult<TAggregate, TEvent>> HandlerForExisting(TCommand command, TAggregate aggregate) =>
        aggregate.MetaData.Version switch
        {
            {Value: 0} => Error.Validation(Constants.InconsistentVersionCode, Constants.InconsistentVersionDescription),
            _ => AggregateCheck(command, aggregate)
                .Match(
                    result => ExecuteCommand(command, aggregate),
                    error => ErrorOr.ErrorOr.From(error).Value)
        };
 
    protected ErrorOr<CommandResult<TAggregate, TEvent>> ApplyEvents(TAggregate aggregate, IEnumerable<TEvent> events)
    {
        var enumerable = events.ToList();
        var aggregatedAggregate = enumerable.Aggregate(aggregate, Apply);

        return new CommandResult<TAggregate, TEvent>(aggregatedAggregate, enumerable);
    }
    
    protected abstract TAggregate Apply(TAggregate aggregate, TEvent @event);

    protected abstract ErrorOr<bool> AggregateCheck(TCommand command, TAggregate aggregate);

    protected abstract ErrorOr<CommandResult<TAggregate, TEvent>> ExecuteCommand(TCommand command, TAggregate aggregate);
}