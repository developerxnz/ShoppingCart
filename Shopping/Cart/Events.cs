using Shopping.Core;
using Shopping.Product;
using Shopping.Product.Core;

namespace Shopping.Cart.Events;

public record CartItemAddedEvent(
        DateTime AddedOnUtc, CustomerId CustomerId, Sku Sku, uint Quantity, Shopping.Core.Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, AddedOnUtc) { }
    
public record CartItemRemovedEvent(
        DateTime RemovedOnUtc, CustomerId CustomerId, Sku Sku, Shopping.Core.Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, RemovedOnUtc) { }
    
public record CartItemUpdatedEvent(
        DateTime UpdatedOnUtc,CustomerId CustomerId, Sku Sku, uint Quantity, Shopping.Core.Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, UpdatedOnUtc) { }
