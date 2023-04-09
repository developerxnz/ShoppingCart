using Shopping.Core;
using Shopping.Delivery.Core;
using Shopping.Product;

namespace Shopping.Cart;

public interface ICartService
{
    /// <summary>
    /// Adds an item to the Cart
    /// </summary>
    /// <param name="sku"></param>
    /// <param name="quantity"></param>
    /// <param name="correlationId"></param>
    /// <returns></returns>
    Task AddToCard(Sku sku, uint quantity, CorrelationId correlationId);

    /// <summary>
    /// Adds an item to the Cart
    /// </summary>
    /// <param name="cartId"></param>
    /// <param name="sku"></param>
    /// <param name="quantity"></param>
    /// <param name="correlationId"></param>
    /// <returns></returns>
    Task AddToCard(CartId cartId, Sku sku, uint quantity, CorrelationId correlationId);
    
    /// <summary>
    /// Removes and item from the Cart
    /// </summary>
    /// <param name="cartId"></param>
    /// <param name="sku"></param>
    /// <param name="correlationId"></param>
    /// <returns></returns>
    Task RemoveFromCart(CartId cartId, Sku sku, CorrelationId correlationId);

    /// <summary>
    /// Updates an Item in the Cart
    /// </summary>
    /// <param name="cartId"></param>
    /// <param name="sku"></param>
    /// <param name="quantity"></param>
    /// <param name="correlationId"></param>
    /// <returns></returns>
    Task UpdateCart(CartId cartId, Sku sku, uint quantity, CorrelationId correlationId);
}

public sealed class CartService : ICartService
{
    private readonly ICartCommandHandler _commandHandler;

    public CartService(ICartCommandHandler commandHandler)
    {
        _commandHandler = commandHandler;
    }

    public Task AddToCard(Sku sku, uint quantity, CorrelationId correlationId)
    {
        AddItemToCartCommand command = new AddItemToCartCommand(DateTime.UtcNow, null, sku, quantity, correlationId);
        var commandResult =_commandHandler.HandlerForNew(command);

        return Task.CompletedTask;
    }

    public Task AddToCard(CartId cartId, Sku sku, uint quantity, CorrelationId correlationId)
    {
        AddItemToCartCommand command = new AddItemToCartCommand(DateTime.UtcNow, cartId, sku, quantity, correlationId);
        var commandResult =_commandHandler.HandlerForNew(command);

        return Task.CompletedTask;
    }

    public Task RemoveFromCart(CartId cartId, Sku sku, CorrelationId correlationId)
    {
        CartAggregate aggregate = new CartAggregate(DateTime.UtcNow);
        RemoveItemFromCartCommand command = new RemoveItemFromCartCommand(DateTime.UtcNow, aggregate.Id, sku, correlationId);
        var commandResult = _commandHandler.HandlerForExisting(command, aggregate);
        
        return Task.CompletedTask;
    }

    public Task UpdateCart(CartId cartId, Sku sku, uint quantity, CorrelationId correlationId)
    {
        CartAggregate aggregate = new CartAggregate(DateTime.UtcNow);
        UpdateItemInCartCommand command = new UpdateItemInCartCommand(DateTime.UtcNow, aggregate.Id, sku, quantity, correlationId);
        var commandResult = _commandHandler.HandlerForExisting(command, aggregate);
        
        return Task.CompletedTask;
    }
}