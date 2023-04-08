using Shopping.Core;

namespace Shopping.Domain.Core.Handlers;

public record Command<T>(CorrelationId CorrelationId, T Data) : ICommand
{
    public CommandId Id { get; } = new (Guid.NewGuid());
}

public record CommandResult<T>(T Aggregate, IEnumerable<Event> Events) : ICommandResult<T, Event>;
