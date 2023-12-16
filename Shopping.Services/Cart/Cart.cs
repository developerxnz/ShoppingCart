using ErrorOr;
using Shopping.Domain.Cart;
using Shopping.Domain.Cart.Commands;
using Shopping.Domain.Cart.Requests;
using Shopping.Domain.Core;
using Shopping.Domain.Product.Core;

namespace Shopping.Services.Cart;

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

public sealed class Cart : Service<CartAggregate, Infrastructure.Persistence.Cart.Cart>, ICart
{
    private readonly ICartCommandHandler _commandHandler;
    private readonly ITransformer<CartAggregate, Infrastructure.Persistence.Cart.Cart> _transformer;

    public Cart(ICartCommandHandler commandHandler, IRepository<Infrastructure.Persistence.Cart.Cart> repository,
        ITransformer<CartAggregate, Infrastructure.Persistence.Cart.Cart> transformer) : base(repository)
    {
        _commandHandler = commandHandler;
        _transformer = transformer;
    }

    public async Task<ErrorOr<AddToCartResponse>> AddToCartAsync(CustomerId customerId, Sku sku, uint quantity,
        CorrelationId correlationId, CancellationToken cancellationToken)
    {
        AddItemToCartCommand command =
            new AddItemToCartCommand(DateTime.UtcNow, customerId, null, sku, quantity, correlationId);
        var commandResult = _commandHandler.HandlerForNew(command);
        return await commandResult
            .MatchAsync<ErrorOr<AddToCartResponse>>(async result =>
                {
                    await SaveAsync(result.Aggregate, result.Events, cancellationToken);

                    return new AddToCartResponse(result.Aggregate.Id, correlationId);
                },
                errors =>
                {
                    ErrorOr<AddToCartResponse> errorResult = ErrorOr.ErrorOr.From(errors).Value;
                    return Task.FromResult(errorResult);
                });
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
            new AddItemToCartCommand(DateTime.UtcNow, request.CustomerId, request.CartId, request.Sku, request.Quantity,
                correlationId);
        var commandResult = _commandHandler.HandlerForExisting(command, aggregateResult.Value);
        return await commandResult
            .MatchAsync<ErrorOr<AddToCartResponse>>(async result =>
                {
                    await SaveAsync(result.Aggregate, result.Events, cancellationToken);

                    return new AddToCartResponse(result.Aggregate.Id, correlationId);
                },
                errors =>
                {
                    ErrorOr<AddToCartResponse> errorResult = ErrorOr.ErrorOr.From(errors).Value;
                    return Task.FromResult(errorResult);
                });
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
        return await commandResult
            .MatchAsync<ErrorOr<RemoveItemFromCartResponse>>(async result =>
                {
                    await SaveAsync(result.Aggregate, result.Events, cancellationToken);

                    return new RemoveItemFromCartResponse(result.Aggregate.Id, correlationId);
                },
                errors =>
                {
                    ErrorOr<RemoveItemFromCartResponse> errorResult = ErrorOr.ErrorOr.From(errors).Value;
                    return Task.FromResult(errorResult);
                });
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

        return await commandResult
            .MatchAsync<ErrorOr<UpdateCartItemResponse>>(async result =>
                {
                    await SaveAsync(result.Aggregate, result.Events, cancellationToken);

                    return new UpdateCartItemResponse(result.Aggregate.Id, correlationId);
                },
                errors =>
                {
                    ErrorOr<UpdateCartItemResponse> errorResult = ErrorOr.ErrorOr.From(errors).Value;
                    return Task.FromResult(errorResult);
                });
    }

    protected override ErrorOr<CartAggregate> ToDomain(Infrastructure.Persistence.Cart.Cart aggregate)
    {
        return _transformer.ToDomain(aggregate);
    }

    protected override (Infrastructure.Persistence.Cart.Cart, IEnumerable<IEvent>) FromDomain(CartAggregate aggregate, IEnumerable<Event> events)
    {
        throw new NotImplementedException();
    }

    // protected Persistence.Cart FromDomain(CartAggregate aggregate)
    // {
    //     return _transformer.FromDomain(aggregate);
    // }
}