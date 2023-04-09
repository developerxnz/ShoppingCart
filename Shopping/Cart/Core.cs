using Shopping.Core;
using Shopping.Domain.Core.Handlers;
using Shopping.Product;

namespace Shopping.Cart;

public interface ICartCommand : ICommand {}

public record CartCommand<T>(CorrelationId CorrelationId, T Data) : Command<T>(CorrelationId, Data), ICartCommand;

public record CartId(Guid Value);

public record CartItem(Sku Sku, uint Quantity);

public record AddItemData(DateTime AddedOnUtc, CartId? CartId, Sku Sku, uint Quantity);
public record AddItemToCartCommand(DateTime AddedOnUtc, CartId? CartId, Sku Sku, uint Quantity, CorrelationId CorrelationId ) 
    : CartCommand<AddItemData>(
        CorrelationId, 
        new AddItemData(AddedOnUtc, CartId, Sku, Quantity)
    );
    
public record RemoveItemData(DateTime RemovedOnUtc, CartId CartId, Sku Sku);
public record RemoveItemFromCartCommand(DateTime RemovedOnUtc, CartId CartId, Sku Sku, CorrelationId CorrelationId ) 
    : CartCommand<RemoveItemData>(
        CorrelationId, 
        new RemoveItemData(RemovedOnUtc, CartId, Sku)
    );
    
public record UpdateItemInCartData(DateTime UpdatedOnUtc, CartId CartId, Sku Sku, uint Quantity);
public record UpdateItemInCartCommand(DateTime UpdatedOnUtc, CartId CartId, Sku Sku, uint Quantity, CorrelationId CorrelationId ) 
    : CartCommand<UpdateItemInCartData>(
        CorrelationId, 
        new UpdateItemInCartData(UpdatedOnUtc, CartId, Sku, Quantity)
    );
    
public record CartItemAddedEvent(
        DateTime AddedOnUtc, Sku Sku, uint Quantity, Shopping.Core.Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, AddedOnUtc) { }
    
public record CartItemRemovedEvent(
        DateTime RemovedOnUtc, Sku Sku, Shopping.Core.Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, RemovedOnUtc) { }
    
public record CartItemUpdatedEvent(
        DateTime UpdatedOnUtc, Sku Sku, uint Quantity, Shopping.Core.Version Version, CorrelationId CorrelationId, CausationId CausationId) 
    : Event(CorrelationId, CausationId, Version, UpdatedOnUtc) { }