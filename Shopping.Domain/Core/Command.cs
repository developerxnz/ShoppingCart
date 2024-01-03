using Shopping.Domain.Core;

namespace Shopping.Domain.Domain.Core.Handlers;

public interface ICommand
{
    CommandId Id { get; }
    
    CorrelationId CorrelationId { get; }
}

public interface ICommandResult<out TAggregate, out TEvent> 
    where TEvent: IEvent
{
    TAggregate Aggregate { get; }

    IEnumerable<TEvent> Events { get; }
}

public record Command<T>(CorrelationId CorrelationId, T Data) : ICommand
{
    public CommandId Id { get; } = new (Guid.NewGuid());
}

public record CommandResult<TAggregate, TEvent>(TAggregate Aggregate, IEnumerable<TEvent> Events) : ICommandResult<TAggregate, TEvent>
where TEvent: IEvent;
