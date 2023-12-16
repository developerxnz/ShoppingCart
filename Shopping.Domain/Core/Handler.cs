using ErrorOr;
using Shopping.Domain.Domain.Core.Handlers;

namespace Shopping.Domain.Core;

public abstract class Handler<T, T1> where T: IAggregate
{
    public abstract ErrorOr<CommandResult<T>> HandlerForNew(T1 command);
    
    public ErrorOr<CommandResult<T>> HandlerForExisting(T1 command, T aggregate) =>
        aggregate.MetaData.Version switch
        {
            {Value: 0} => Error.Validation(Constants.InconsistentVersionCode, Constants.InconsistentVersionDescription),
            _ => AggregateCheck(command, aggregate)
                .Match(
                    result => ExecuteCommand(command, aggregate),
                    error => ErrorOr.ErrorOr.From(error).Value)
        };
 
    protected ErrorOr<CommandResult<T>> ApplyEvents(T aggregate, IEnumerable<Event> events)
    {
        var enumerable = events.ToList();
        var aggregatedAggregate = enumerable.Aggregate(aggregate, Apply);

        return new CommandResult<T>(aggregatedAggregate, enumerable);
    }
    
    protected abstract T Apply(T aggregate, IEvent @event);

    protected abstract ErrorOr<bool> AggregateCheck(T1 command, T aggregate);

    protected abstract ErrorOr<CommandResult<T>> ExecuteCommand(T1 command, T aggregate);
}