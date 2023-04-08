using ErrorOr;
using Shopping.Domain.Core.Handlers;

namespace Shopping.Core;

public abstract class Handler<T>
{
    protected ErrorOr<CommandResult<T>> ApplyEvents(T aggregate, IEnumerable<Event> events)
    {
        var enumerable = events.ToList();
        var x = enumerable.Aggregate(aggregate, Apply);

        return new CommandResult<T>(x, enumerable);
    }
    
    protected abstract T Apply(T aggregate, IEvent @event);
}