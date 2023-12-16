using Shopping.Domain.Cart.Projections;
using Shopping.Domain.Core;
using Shopping.Domain.Product.Core;
using Version = Shopping.Domain.Core.Version;

namespace Shopping.Domain.Product.Events;

public interface IProductEvent {}

public record ProductCreatedEvent(
        CorrelationId CorrelationId,
        CausationId CausationId,
        DateTime CreatedOnUtc,
        ProductId ProductId,
        ProductDescription Description,
        ProductPrice Price,
        Sku Sku,
        Version Version)
    : Event(CorrelationId, CausationId, Version, CreatedOnUtc), IProductEvent;
    
public record ProductUpdatedEvent(
        CorrelationId CorrelationId,
        CausationId CausationId,
        DateTime UpdatedOnUtc,
        ProductId ProductId,
        ProductDescription Description,
        ProductPrice Price,
        Sku Sku,
        Version Version)
    : Event(CorrelationId, CausationId, Version, UpdatedOnUtc), IProductEvent;