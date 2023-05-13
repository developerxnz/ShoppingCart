using ErrorOr;
using Shopping.Cart.Commands;
using Shopping.Cart.Requests;
using Shopping.Core;
using Shopping.Product;

namespace Shopping.Cart.Services;

public interface ICart
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
    Task<ErrorOr<AddToCartResponse>> AddToCartAsync(CustomerId customerId, Sku sku, uint quantity,
        CorrelationId correlationId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds an item to the Cart
    /// </summary>
    /// <param name="request"></param>
    /// <param name="correlationId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<AddToCartResponse>> AddToCartAsync(AddToCartRequest request, CorrelationId correlationId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Removes and item from the Cart
    /// </summary>
    /// <param name="request"></param>
    /// <param name="correlationId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<RemoveItemFromCartResponse>> RemoveFromCartAsync(RemoveItemFromCartRequest request,
        CorrelationId correlationId, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an Item in the Cart
    /// </summary>
    /// <param name="request"></param>
    /// <param name="correlationId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<UpdateCartItemResponse>> UpdateCartAsync(UpdateCartItemRequest request, CorrelationId correlationId,
        CancellationToken cancellationToken);
}

public sealed class Cart : Service<CartAggregate, Persistence.Cart>, ICart
{
    private readonly ICartCommandHandler _commandHandler;
    private readonly ITransformer<CartAggregate, Persistence.Cart> _transformer;

    public Cart(ICartCommandHandler commandHandler, IRepository<Persistence.Cart> repository,
        ITransformer<CartAggregate, Persistence.Cart> transformer) : base(repository)
    {
        _commandHandler = commandHandler;
        _transformer = transformer;
    }

    public async Task<ErrorOr<AddToCartResponse>> AddToCartAsync(CustomerId customerId, Sku sku, uint quantity,
        CorrelationId correlationId, CancellationToken cancellationToken)
    {
        AddItemToCartCommand command = new AddItemToCartCommand(DateTime.UtcNow, customerId, null, sku, quantity, correlationId);
        var commandResult = _commandHandler.HandlerForNew(command);
        if (commandResult.IsError)
        {
            return ErrorOr.ErrorOr.From(commandResult.Errors).Value;
        }

        await SaveAsync(commandResult.Value.Aggregate, commandResult.Value.Events, cancellationToken);

        return new AddToCartResponse(commandResult.Value.Aggregate.Id, correlationId);
    }

    public async Task<ErrorOr<AddToCartResponse>> AddToCartAsync(AddToCartRequest request, CorrelationId correlationId,
        CancellationToken cancellationToken)
    {
        PartitionKey partitionKey = new PartitionKey(request.CustomerId.Value.ToString());
        Id id = new Id(request.CartId.Value.ToString());
        var aggregateResult = await LoadAsync(partitionKey, id, cancellationToken);
        if (aggregateResult.IsError)
        {
            return ErrorOr.ErrorOr.From(aggregateResult.Errors).Value;
        }

        AddItemToCartCommand command = 
            new AddItemToCartCommand(DateTime.UtcNow, request.CustomerId, request.CartId, request.Sku, request.Quantity, correlationId);
        var commandResult = _commandHandler.HandlerForExisting(command, aggregateResult.Value);
        if (commandResult.IsError)
        {
            return ErrorOr.ErrorOr.From(commandResult.Errors).Value;
        }

        await SaveAsync(commandResult.Value.Aggregate, commandResult.Value.Events, cancellationToken);

        return new AddToCartResponse(commandResult.Value.Aggregate.Id, correlationId);
    }

    public async Task<ErrorOr<RemoveItemFromCartResponse>> RemoveFromCartAsync(RemoveItemFromCartRequest request,
        CorrelationId correlationId, CancellationToken cancellationToken)
    {
        PartitionKey partitionKey = new PartitionKey(request.CustomerId.Value.ToString());
        Id id = new Id(request.CartId.Value.ToString());
        var aggregateResult = await LoadAsync(partitionKey, id, cancellationToken);
        if (aggregateResult.IsError)
        {
            return ErrorOr.ErrorOr.From(aggregateResult.Errors).Value;
        }

        RemoveItemFromCartCommand command = new RemoveItemFromCartCommand(DateTime.UtcNow, request.CustomerId,
            aggregateResult.Value.Id, request.Sku, correlationId);
        var commandResult = _commandHandler.HandlerForExisting(command, aggregateResult.Value);
        if (commandResult.IsError)
        {
            return ErrorOr.ErrorOr.From(commandResult.Errors).Value;
        }

        await SaveAsync(commandResult.Value.Aggregate, commandResult.Value.Events, cancellationToken);

        return new RemoveItemFromCartResponse(commandResult.Value.Aggregate.Id, correlationId);
    }

    public async Task<ErrorOr<UpdateCartItemResponse>> UpdateCartAsync(UpdateCartItemRequest request,
        CorrelationId correlationId, CancellationToken cancellationToken)
    {
        PartitionKey partitionKey = new PartitionKey(request.CustomerId.Value.ToString());
        Id id = new Id(request.CartId.Value.ToString());
        var aggregateResult = await LoadAsync(partitionKey, id, cancellationToken);
        if (aggregateResult.IsError)
        {
            return ErrorOr.ErrorOr.From(aggregateResult.Errors).Value;
        }

        UpdateItemInCartCommand command = new UpdateItemInCartCommand(DateTime.UtcNow, request.CustomerId,
            aggregateResult.Value.Id, request.Sku, request.Quantity, correlationId);
        var commandResult = _commandHandler.HandlerForExisting(command, aggregateResult.Value);
        if (commandResult.IsError)
        {
            return ErrorOr.ErrorOr.From(commandResult.Errors).Value;
        }

        await SaveAsync(commandResult.Value.Aggregate, commandResult.Value.Events, cancellationToken);

        return new UpdateCartItemResponse(commandResult.Value.Aggregate.Id, correlationId);
    }

    protected override ErrorOr<CartAggregate> ToDomain(Persistence.Cart persistenceAggregate)
    {
        return _transformer.ToDomain(persistenceAggregate);
    }

    protected override Persistence.Cart FromDomain(CartAggregate aggregate)
    {
        return _transformer.FromDomain(aggregate);
    }
}