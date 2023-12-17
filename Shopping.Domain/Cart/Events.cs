using Shopping.Domain.Core;
using Shopping.Domain.Product.Core;
using Version = Shopping.Domain.Core.Version;

namespace Shopping.Domain.Cart.Events;

public record CartItemAddedEvent(
        DateTime AddedOnUtc, CustomerId CustomerId, Sku Sku, CartQuantity Quantity, Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, AddedOnUtc) { }
    
public record CartItemRemovedEvent(
        DateTime RemovedOnUtc, CustomerId CustomerId, Sku Sku, Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, RemovedOnUtc) { }
    
public record CartItemUpdatedEvent(
        DateTime UpdatedOnUtc,CustomerId CustomerId, Sku Sku, CartQuantity Quantity, Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, UpdatedOnUtc) { }
