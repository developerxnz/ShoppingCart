using ErrorOr;
using Shopping.Domain.Core;
using Shopping.Infrastructure.Interfaces;
using IEvent = Shopping.Domain.Core.IEvent;

namespace Shopping.Services;

public abstract class Service<TAggregate, TPersistenceAggregate, TEvent> 
    where TPersistenceAggregate:IPersistenceIdentifier
    where TEvent: IEvent
{
    private readonly IRepository<TPersistenceAggregate> _repository;

    protected Service(IRepository<TPersistenceAggregate> repository)
    {
        _repository = repository;
    }

    protected abstract ErrorOr<TAggregate> ToDomain(TPersistenceAggregate aggregate);
    
    protected abstract (TPersistenceAggregate, IEnumerable<Shopping.Infrastructure.Interfaces.IEvent>) FromDomain(TAggregate aggregate, IEnumerable<TEvent> events);
    
    protected async Task<ErrorOr<TAggregate>> LoadAsync(PartitionKey partitionKey, Id id, CancellationToken cancellationToken)
    {
        TPersistenceAggregate response = await _repository.GetByIdAsync(partitionKey.Value, id.Value, cancellationToken);
        
        return ToDomain(response);
    }
    
    protected async Task SaveAsync(TAggregate aggregate, IEnumerable<TEvent> events, CancellationToken cancellationToken)
    {
        var (aggregateDto, eventDtos) = FromDomain(aggregate, events);
        
        await _repository.BatchUpdateAsync(aggregateDto, eventDtos, cancellationToken);
    }
}