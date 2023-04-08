using ErrorOr;
using Shopping.Domain.Core.Handlers;

namespace Shopping.Core;

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