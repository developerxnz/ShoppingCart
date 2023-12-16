using Shopping.Domain.Core;

namespace Shopping.Domain.Domain.Core.Handlers;

public interface ICommand
{
    CommandId Id { get; }
    
    CorrelationId CorrelationId { get; }
}

public interface ICommandResult<out T1, out T2> where T2: IEvent
{
    T1 Aggregate { get; }

    IEnumerable<T2> Events { get; }
}

public record Command<T>(CorrelationId CorrelationId, T Data) : ICommand
{
    public CommandId Id { get; } = new (Guid.NewGuid());
}

public record CommandResult<T>(T Aggregate, IEnumerable<Event> Events) : ICommandResult<T, Event>;
