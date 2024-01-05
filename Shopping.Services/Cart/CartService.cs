using ErrorOr;
using Shopping.Domain.Cart;
using Shopping.Domain.Cart.Commands;
using Shopping.Domain.Cart.Events;
using Shopping.Domain.Cart.Requests;
using Shopping.Domain.Core;
using Shopping.Domain.Product.Core;
using Shopping.Infrastructure.Interfaces;
using Shopping.Services.Interfaces;
using IEvent = Shopping.Infrastructure.Interfaces.IEvent;

namespace Shopping.Services.Cart;

public sealed class Cart : Service<Domain.Cart.Cart, Infrastructure.Persistence.Cart.Cart, ICartEvent>, ICartService
{
    private readonly ICartCommandHandler _commandHandler;
    private readonly IMapper<Domain.Cart.Cart, Infrastructure.Persistence.Cart.Cart, 
        Domain.Cart.Events.ICartEvent, Infrastructure.Persistence.Cart.CartEvent> _mapper;

    public Cart(ICartCommandHandler commandHandler,
        IRepository<Infrastructure.Persistence.Cart.Cart> repository,
        IMapper<Domain.Cart.Cart, Infrastructure.Persistence.Cart.Cart, 
            Domain.Cart.Events.ICartEvent, Infrastructure.Persistence.Cart.CartEvent> mapper) : base(repository)
    {
        _mapper = mapper;
        _commandHandler = commandHandler;
    }

    public Task<ErrorOr<AddToCartResponse>> AddToCartAsync(CustomerId customerId, Sku sku, CartQuantity quantity,
        CorrelationId correlationId, CancellationToken cancellationToken)
    {
        AddItemToCartCommand command =
            new AddItemToCartCommand(DateTime.UtcNow, customerId, null, sku, quantity, correlationId);

        var commandResult = _commandHandler.HandlerForNew(command);
        return commandResult
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

        AddItemToCartCommand command = new AddItemToCartCommand(DateTime.UtcNow, request.CustomerId, request.CartId, request.Sku, request.Quantity, correlationId);
        
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

    protected override ErrorOr<Domain.Cart.Cart> ToDomain(Infrastructure.Persistence.Cart.Cart aggregate) 
        => _mapper.ToDomain(aggregate);

    protected override (Infrastructure.Persistence.Cart.Cart, IEnumerable<IEvent>) 
        FromDomain(Domain.Cart.Cart aggregate, IEnumerable<ICartEvent> events)
    {
        return _mapper.FromDomain(aggregate, events);
    }
}