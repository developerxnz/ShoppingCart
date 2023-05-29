namespace Shopping.Core;
using ErrorOr;

public abstract class Service<TAggregate, TPersistenceAggregate> where TPersistenceAggregate:IPersistenceIdentifier
{
    private protected readonly IRepository<TPersistenceAggregate> Repository;

    protected Service(IRepository<TPersistenceAggregate> repository)
    {
        Repository = repository;
    }

    protected abstract ErrorOr<TAggregate> ToDomain(TPersistenceAggregate aggregate);
    
    protected abstract TPersistenceAggregate FromDomain(TAggregate aggregate);
    
    protected async Task<ErrorOr<TAggregate>> LoadAsync(PartitionKey partitionKey, Id id, CancellationToken cancellationToken)
    {
        TPersistenceAggregate response = await Repository.GetByIdAsync(partitionKey.Value, id.Value, cancellationToken);
        return ToDomain(response);
    }
    
    protected async Task SaveAsync(TAggregate aggregate, IEnumerable<Event> events, CancellationToken cancellationToken)
    {
        var transformedAggregate = FromDomain(aggregate);
        await Repository.BatchUpdateAsync(transformedAggregate, events, cancellationToken);
    }
}