using Shopping.Core;
using Shopping.Domain.Core.Handlers;
using Shopping.Product;

namespace Shopping.Cart;

public interface ICartCommand : ICommand {}

public record CartCommand<T>(CorrelationId CorrelationId, T Data) : Command<T>(CorrelationId, Data), ICartCommand;

public record CartId(Guid Value);

public record CartItem(Sku Sku, uint Quantity);

public record AddItemData(DateTime AddedOnUtc, CustomerId CustomerId, CartId? CartId, Sku Sku, uint Quantity);
public record AddItemToCartCommand(DateTime AddedOnUtc, CustomerId CustomerId, CartId? CartId, Sku Sku, uint Quantity, CorrelationId CorrelationId ) 
    : CartCommand<AddItemData>(
        CorrelationId, 
        new AddItemData(AddedOnUtc, CustomerId, CartId, Sku, Quantity)
    );
    
public record RemoveItemData(DateTime RemovedOnUtc, CustomerId CustomerId, CartId CartId, Sku Sku);
public record RemoveItemFromCartCommand(DateTime RemovedOnUtc, CustomerId CustomerId, CartId CartId, Sku Sku, CorrelationId CorrelationId ) 
    : CartCommand<RemoveItemData>(
        CorrelationId, 
        new RemoveItemData(RemovedOnUtc, CustomerId, CartId, Sku)
    );
    
public record UpdateItemInCartData(DateTime UpdatedOnUtc, CustomerId CustomerId, CartId CartId, Sku Sku, uint Quantity);
public record UpdateItemInCartCommand(DateTime UpdatedOnUtc, CustomerId CustomerId, CartId CartId, Sku Sku, uint Quantity, CorrelationId CorrelationId ) 
    : CartCommand<UpdateItemInCartData>(
        CorrelationId, 
        new UpdateItemInCartData(UpdatedOnUtc, CustomerId, CartId, Sku, Quantity)
    );
    
public record CartItemAddedEvent(
        DateTime AddedOnUtc, CustomerId CustomerId, Sku Sku, uint Quantity, Shopping.Core.Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, AddedOnUtc) { }
    
public record CartItemRemovedEvent(
        DateTime RemovedOnUtc, CustomerId CustomerId, Sku Sku, Shopping.Core.Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, RemovedOnUtc) { }
    
public record CartItemUpdatedEvent(
        DateTime UpdatedOnUtc,CustomerId CustomerId, Sku Sku, uint Quantity, Shopping.Core.Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, UpdatedOnUtc) { }
    
    
public record UpdateCartItemRequest(CustomerId CustomerId, CartId CartId, Sku Sku, uint Quantity);
public record UpdateCartItemResponse(CartId CartId, CorrelationId CorrelationId);

public record RemoveItemFromCartRequest(CustomerId CustomerId, CartId CartId, Sku Sku);

public record RemoveItemFromCartResponse(CartId CartId, CorrelationId CorrelationId);

public record AddToCartRequest(CustomerId CustomerId, CartId CartId, Sku Sku, uint Quantity);

public record AddToCartResponse(CartId CartId, CorrelationId CorrelationId);
