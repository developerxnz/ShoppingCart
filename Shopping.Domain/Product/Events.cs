using Shopping.Domain.Core;
using Shopping.Domain.Product.Core;
using Version = Shopping.Domain.Core.Version;

namespace Shopping.Domain.Product.Events;

public interface IProductEvent : IEvent {}

public abstract record ProductEvent(DateTime ModifiedDateUtc, Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, ModifiedDateUtc), IProductEvent;

public record ProductCreatedEvent(
        CorrelationId CorrelationId,
        CausationId CausationId,
        DateTime CreatedOnUtc,
        ProductId ProductId,
        ProductDescription Description,
        ProductPrice Price,
        Sku Sku,
        Version Version)
    : ProductEvent(CreatedOnUtc, Version, CorrelationId, CausationId), IProductEvent;
    
public record ProductUpdatedEvent(
        CorrelationId CorrelationId,
        CausationId CausationId,
        DateTime UpdatedOnUtc,
        ProductId ProductId,
        ProductDescription Description,
        ProductPrice Price,
        Sku Sku,
        Version Version)
    : ProductEvent(UpdatedOnUtc, Version, CorrelationId, CausationId), IProductEvent;