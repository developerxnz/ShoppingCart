using Shopping.Cart.Projections;
using Shopping.Core;
using Shopping.Product.Core;

namespace Shopping.Product.Events;

public interface IProductEvent {}

public record ProductCreatedEvent(
        CorrelationId CorrelationId,
        CausationId CausationId,
        DateTime CreatedOnUtc,
        ProductId ProductId,
        ProductDescription Description,
        ProductPrice Price,
        Sku Sku,
        Shopping.Core.Version Version)
    : Event(CorrelationId, CausationId, Version, CreatedOnUtc), IProductEvent;
    
public record ProductUpdatedEvent(
        CorrelationId CorrelationId,
        CausationId CausationId,
        DateTime UpdatedOnUtc,
        ProductId ProductId,
        ProductDescription Description,
        ProductPrice Price,
        Sku Sku,
        Shopping.Core.Version Version)
    : Event(CorrelationId, CausationId, Version, UpdatedOnUtc), IProductEvent;