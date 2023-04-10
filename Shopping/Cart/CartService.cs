using ErrorOr;
using Shopping.Core;
using Shopping.Delivery.Core;
using Shopping.Product;

namespace Shopping.Cart;

public interface ICartService
{
    /// <summary>
    /// Adds an item to the Cart
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="sku"></param>
    /// <param name="quantity"></param>
    /// <param name="correlationId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<AddToCartResponse>> AddToCart(CustomerId customerId, Sku sku, uint quantity, CorrelationId correlationId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds an item to the Cart
    /// </summary>
    /// <param name="request"></param>
    /// <param name="correlationId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<AddToCartResponse>> AddToCart(AddToCartRequest request, CorrelationId correlationId,CancellationToken cancellationToken);

    /// <summary>
    /// Removes and item from the Cart
    /// </summary>
    /// <param name="request"></param>
    /// <param name="correlationId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<RemoveItemFromCartResponse>> RemoveFromCart(RemoveItemFromCartRequest request, CorrelationId correlationId, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an Item in the Cart
    /// </summary>
    /// <param name="request"></param>
    /// <param name="correlationId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<UpdateCartItemResponse>> UpdateCart(UpdateCartItemRequest request, CorrelationId correlationId, CancellationToken cancellationToken);
}

public sealed class CartService : ICartService
{
    private readonly ICartCommandHandler _commandHandler;
    private readonly IRepository<Persistence.Cart, IEvent> _repository;
    private readonly ITransformer<CartAggregate, Persistence.Cart> _transformer;

    public CartService(ICartCommandHandler commandHandler, IRepository<Persistence.Cart, IEvent> repository, ITransformer<CartAggregate, Persistence.Cart> transformer)
    {
        _commandHandler = commandHandler;
        _repository = repository;
        _transformer = transformer;
    }

    public async Task<ErrorOr<AddToCartResponse>> AddToCart(CustomerId customerId, Sku sku, uint quantity, CorrelationId correlationId, CancellationToken cancellationToken)
    {
        AddItemToCartCommand command = new AddItemToCartCommand(DateTime.UtcNow, customerId, null, sku, quantity, correlationId);
        var commandResult =_commandHandler.HandlerForNew(command);
        if (commandResult.IsError)
        {
            return ErrorOr.ErrorOr.From(commandResult.Errors).Value;
        }
        
        await SaveAsync(commandResult.Value.Aggregate, commandResult.Value.Events, cancellationToken);

        return new AddToCartResponse(commandResult.Value.Aggregate.Id, correlationId);
    }

    public async Task<ErrorOr<AddToCartResponse>> AddToCart(AddToCartRequest request, CorrelationId correlationId, CancellationToken cancellationToken)
    {
        var aggregateResult = await LoadAsync(request.CustomerId, request.CartId, cancellationToken);
        if (aggregateResult.IsError)
        {
            return ErrorOr.ErrorOr.From(aggregateResult.Errors).Value;
        }
        
        AddItemToCartCommand command = new AddItemToCartCommand(DateTime.UtcNow, request.CustomerId, request.CartId, request.Sku, request.Quantity, correlationId);
        var commandResult =_commandHandler.HandlerForNew(command);
        if (commandResult.IsError)
        {
            return ErrorOr.ErrorOr.From(commandResult.Errors).Value;
        }
        
        await SaveAsync(commandResult.Value.Aggregate, commandResult.Value.Events, cancellationToken);
        
        return new AddToCartResponse(commandResult.Value.Aggregate.Id, correlationId);
    }

    public async Task<ErrorOr<RemoveItemFromCartResponse>> RemoveFromCart(RemoveItemFromCartRequest request, CorrelationId correlationId, CancellationToken cancellationToken)
    {
        var aggregateResult = await LoadAsync(request.CustomerId, request.CartId, cancellationToken);
        if (aggregateResult.IsError)
        {
            return ErrorOr.ErrorOr.From(aggregateResult.Errors).Value;
        }
        
        RemoveItemFromCartCommand command = new RemoveItemFromCartCommand(DateTime.UtcNow, request.CustomerId, aggregateResult.Value.Id, request.Sku, correlationId);
        var commandResult = _commandHandler.HandlerForExisting(command, aggregateResult.Value);
        if (commandResult.IsError)
        {
            return ErrorOr.ErrorOr.From(commandResult.Errors).Value;
        }
        
        await SaveAsync(commandResult.Value.Aggregate, commandResult.Value.Events, cancellationToken);
        
        return new RemoveItemFromCartResponse(commandResult.Value.Aggregate.Id, correlationId);
    }

    public async Task<ErrorOr<UpdateCartItemResponse>> UpdateCart(UpdateCartItemRequest request, CorrelationId correlationId, CancellationToken cancellationToken)
    {
        var aggregateResult = await LoadAsync(request.CustomerId, request.CartId, cancellationToken);
        if (aggregateResult.IsError)
        {
            return ErrorOr.ErrorOr.From(aggregateResult.Errors).Value;
        }
        
        UpdateItemInCartCommand command = new UpdateItemInCartCommand(DateTime.UtcNow, request.CustomerId, aggregateResult.Value.Id, request.Sku, request.Quantity, correlationId);
        var commandResult = _commandHandler.HandlerForExisting(command, aggregateResult.Value);
        if (commandResult.IsError)
        {
            return ErrorOr.ErrorOr.From(commandResult.Errors).Value;
        }
        
        await SaveAsync(commandResult.Value.Aggregate, commandResult.Value.Events, cancellationToken);

        return new UpdateCartItemResponse(commandResult.Value.Aggregate.Id, correlationId);
    }
    
    private async Task<ErrorOr<CartAggregate>> LoadAsync(CustomerId customerId, CartId cartId, CancellationToken cancellationToken)
    {
        Persistence.Cart response = await _repository.GetByIdAsync(customerId.Value.ToString(), cartId.Value.ToString(), cancellationToken);
        return _transformer.ToDomain(response);
    }
    
    private async Task SaveAsync(CartAggregate aggregate, IEnumerable<Event> events, CancellationToken cancellationToken)
    {
        var transformedAggregate = _transformer.FromDomain(aggregate);
        await _repository.BatchUpdate(transformedAggregate.CustomerId, transformedAggregate, events);
    }
}