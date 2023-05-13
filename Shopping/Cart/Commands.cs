using Shopping.Cart.Core;
using Shopping.Core;
using Shopping.Domain.Core.Handlers;
using Shopping.Product;

namespace Shopping.Cart.Commands;

public interface ICartCommand : ICommand {}

public record CartCommand<T>(CorrelationId CorrelationId, T Data) : Command<T>(CorrelationId, Data), ICartCommand;

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
