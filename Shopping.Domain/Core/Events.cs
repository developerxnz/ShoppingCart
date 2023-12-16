namespace Shopping.Domain.Core;

public interface IEvent
{
    Version Version { get; }
    
    CausationId CausationId { get; }
    
    CorrelationId CorrelationId { get; }
    
    DateTime TimeStamp { get; }
}

public record Event(CorrelationId CorrelationId, CausationId CausationId, Version Version, DateTime TimeStamp) : IEvent
{
    public EventId Id { get; } = new(Guid.NewGuid());
}