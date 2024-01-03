using Shopping.Domain.Core;
using Shopping.Domain.Product.Core;
using Version = Shopping.Domain.Core.Version;

namespace Shopping.Domain.Cart.Events;

public abstract record CartEvent(DateTime ModifiedDateUtc, Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, ModifiedDateUtc), ICartEvent;

public record CartItemAddedEvent(
        DateTime AddedOnUtc, CustomerId CustomerId, Sku Sku, CartQuantity Quantity, Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : CartEvent (AddedOnUtc, Version, CorrelationId, CausationId) { }
    
public record CartItemRemovedEvent(
        DateTime RemovedOnUtc, CustomerId CustomerId, Sku Sku, Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : CartEvent (RemovedOnUtc, Version, CorrelationId, CausationId) { }
    
public record CartItemUpdatedEvent(
        DateTime UpdatedOnUtc,CustomerId CustomerId, Sku Sku, CartQuantity Quantity, Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : CartEvent (UpdatedOnUtc, Version, CorrelationId, CausationId) { }

public interface ICartEvent : IEvent { }